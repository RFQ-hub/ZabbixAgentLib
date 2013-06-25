using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using NLog;

namespace Ids.ZabbixAgent
{
    public class PassiveCheckServer : IDisposable
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private const string NOT_SUPPORTED = "ZBX_NOTSUPPORTED";
        private const string HEADER_STRING = "ZBXD\x01";
        private static readonly byte[] HeaderBytes = Encoding.ASCII.GetBytes(HEADER_STRING);

        private readonly TcpListener server;

        public PassiveCheckServer(IPEndPoint endpoint)
        {
            if (endpoint == null) throw new ArgumentNullException("endpoint");

            server = new TcpListener(endpoint);
        }

        private bool active;

        public void Start()
        {
            if (active) return;

            Log.Info("Starting passive server on {0}", server.LocalEndpoint);
            server.Start();
            active = true;
            BeginAcceptTcpClient();
        }

        private void BeginAcceptTcpClient()
        {
            if (!active) return;
            Log.Trace("Listening for new connections");
            server.BeginAcceptTcpClient(ClientConnectedCallback, null);
        }

        private static readonly Regex KeyRegex = new Regex(@"^(?<key>[^\[]*)(\[(?<args>.*)\])?",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private delegate object GetItemMethod(string args);

        public delegate T TypedGetItemMethodWithArgs<out T>(string args);
        public delegate T TypedGetItemMethod<out T>();

        private readonly Dictionary<string, GetItemMethod> items = new Dictionary<string, GetItemMethod>();

        public void AddItem<T>(string item, TypedGetItemMethodWithArgs<T> getItem)
        {
            items.Add(item, args => getItem(args));
        }

        public void AddItem<T>(string item, TypedGetItemMethod<T> getItem)
        {
            items.Add(item, args => getItem());
        }

        public event EventHandler<ClientConnectedEventArgs> ClientConnected;

        private void RaiseClientConnected(IPAddress address, out bool denyConnection)
        {
            var deleg = ClientConnected;

            if (deleg != null)
            {
                var args = new ClientConnectedEventArgs(address);
                deleg(this, args);
                denyConnection = args.DenyConnection;
            }
            else
            {
                denyConnection = false;
            }
        }

        private void ClientConnectedCallback(IAsyncResult ar)
        {
            try
            {
                TcpClient tcpClient;
                try
                {
                    tcpClient = server.EndAcceptTcpClient(ar);
                }
                catch (ObjectDisposedException)
                {
                    return;
                }

                using (tcpClient)
                {
                    BeginAcceptTcpClient();

                    ClientConnectedCallbackCore(tcpClient);
                }
            }
            catch (Exception exception)
            {
                Log.Fatal("Uncatched exception in ClientConnectedCallback", exception);
            }
        }

        private void ClientConnectedCallbackCore(TcpClient tcpClient)
        {
            var ipEndpoint = (IPEndPoint) tcpClient.Client.RemoteEndPoint;
            bool denyConnection;
            RaiseClientConnected(ipEndpoint.Address, out denyConnection);
            if (denyConnection)
            {
                Log.Trace("Denying connection from {0}", ipEndpoint.Address);
                return;
            }

            Log.Trace("Accepted connection from {0}", ipEndpoint.Address);
            using (var stream = tcpClient.GetStream())
            {
                ReadKeyAndWriteAnswer(stream);
            }
        }

        private void ReadKeyAndWriteAnswer(NetworkStream stream)
        {
            string key;
            string args;
            if (!TryReadKey(stream, out key, out args))
            {
                return;
            }

            Log.Info("Requested item '{0}' with args '{1}'", key, args);

            var valueString = GetItemStringValue(key, args);

            Log.Info("Answering: {0}", valueString);

            WriteAnswer(stream, valueString);
        }

        private static void WriteAnswer(Stream stream, string valueString)
        {
            var valueStringBytes = Encoding.UTF8.GetBytes(valueString);

            stream.Write(HeaderBytes, 0, HeaderBytes.Length);
            var sizeBytes = BitConverter.GetBytes((long) valueStringBytes.Length);
            stream.Write(sizeBytes, 0, sizeBytes.Length);
            stream.Write(valueStringBytes, 0, valueStringBytes.Length);
        }

        private static bool TryReadKey(Stream stream, out string key, out string args)
        {
            var streamReader = new StreamReader(stream);
            string rawKey = streamReader.ReadLine();

            Log.Trace("Received: {0}", rawKey);

            if (rawKey == null)
            {
                key = null;
                args = null;
                return false;
            }

            ParseZabbixKey(rawKey, out key, out args);
            return true;
        }

        private static void ParseZabbixKey(string rawKey, out string key, out string args)
        {
            var keyMatch = KeyRegex.Match(rawKey);

            key = keyMatch.Groups["key"].Value;
            args = keyMatch.Groups["args"].Value;
        }

        private string GetItemStringValue(string key, string args)
        {
            object value;
            GetItemMethod getItemMethod;
            if (items.TryGetValue(key, out getItemMethod))
            {
                try
                {
                    value = getItemMethod(args);
                }
                catch (Exception exception)
                {
                    var message = string.Format("Unable to get item '{0}' with args '{1}'", items, args);
                    Log.ErrorException(message, exception);

                    value = NOT_SUPPORTED; // TODO: Find how to specify an error
                }
            }
            else
            {
                value = NOT_SUPPORTED;
            }

            var valueString = Convert.ToString(value, CultureInfo.InvariantCulture);
            return valueString;
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    Stop();
                }
                catch (Exception exception)
                {
                    Log.Error("Exception during server stop", exception);
                }
            }
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
        }

        public void Stop()
        {
            if (active)
            {
                server.Stop();
                active = false;
            }
        }
    }
}
