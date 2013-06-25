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
    public class PassiveCheckServer
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

        public void Start()
        {
            Log.Info("Starting passive server on {0}", server.LocalEndpoint);
            server.Start();
            BeginAcceptTcpClient();
        }

        private void BeginAcceptTcpClient()
        {
            Log.Trace("Listening for new connections");
            server.BeginAcceptTcpClient(ClientConnected, null);
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

        private void ClientConnected(IAsyncResult ar)
        {
            BeginAcceptTcpClient();

            using (var tcpClient = server.EndAcceptTcpClient(ar))
            {
                using (var stream = tcpClient.GetStream())
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
            }
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
                value = getItemMethod(args);
            }
            else
            {
                value = NOT_SUPPORTED;
            }

            var valueString = Convert.ToString(value, CultureInfo.InvariantCulture);
            return valueString;
        }
    }
}
