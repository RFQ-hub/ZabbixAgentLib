using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Itg.ZabbixAgentLib.Core;
using NLog;

namespace Itg.ZabbixAgentLib
{
    public abstract class PassiveCheckServerBase : IDisposable
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly TcpListener server;

        protected PassiveCheckServerBase(IPEndPoint endpoint)
        {
            if (endpoint == null)
            {
                throw new ArgumentNullException("endpoint");
            }

            server = new TcpListener(endpoint);
        }

        private bool active;

        public void Start()
        {
            if (active)
            {
                return;
            }

            log.Info("Starting passive server on {0}", server.LocalEndpoint);
            server.Start();
            active = true;
            BeginAcceptTcpClient();
        }

        private void BeginAcceptTcpClient()
        {
            if (!active)
            {
                return;
            }

            log.Trace("Listening for new connections");
            server.BeginAcceptTcpClient(OnClientConnected, null);
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

        private void OnClientConnected(IAsyncResult asyncResult)
        {
            try
            {
                // Accept the connection
                TcpClient tcpClient;
                try
                {
                    tcpClient = server.EndAcceptTcpClient(asyncResult);
                }
                catch (ObjectDisposedException)
                {
                    return;
                }

                // Asynchronously accept the next connection
                BeginAcceptTcpClient();

                // Handle the request
                using (tcpClient)
                {
                    HandleClient(tcpClient);
                }
            }
            catch (Exception exception)
            {
                log.FatalException("Uncatched exception in OnClientConnected", exception);
            }
        }

        private void HandleClient(TcpClient tcpClient)
        {
            var ipEndpoint = (IPEndPoint)tcpClient.Client.RemoteEndPoint;
            RaiseClientConnected(ipEndpoint.Address, out var denyConnection);
            if (denyConnection)
            {
                log.Trace("Denying connection from {0}", ipEndpoint.Address);
                return;
            }

            log.Trace("Accepted connection from {0}", ipEndpoint.Address);
            using (var stream = tcpClient.GetStream())
            {
                ReadKeyAndWriteAnswer(stream);
            }
        }

        private void ReadKeyAndWriteAnswer(NetworkStream stream)
        {
            var streamReader = new StreamReader(stream);
            var key = streamReader.ReadLine();
            log.Trace("Received: {0}", key);

            var valueString = GetItemStringValue(key);

            log.Info("Answering: {0}", valueString);

            ZabbixProtocol.WriteWithHeader(stream, valueString);
        }

        protected abstract PassiveCheckResult GetValue(string key, string args);

        private string GetItemStringValue(string key)
        {
            PassiveCheckResult value;

            ZabbixKeyParser.Parse(key, out key, out var args);

            try
            {
                value = GetValue(key, args);
            }
            catch (Exception exception)
            {
                var message = string.Format("Unable to get item '{0}'", key);
                log.ErrorException(message, exception);

                value = PassiveCheckResult.NotSupported;
            }

            return value.IsNotSupported ? ZabbixConstants.NotSupported : value.Value;
        }

        protected void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            try
            {
                Stop();
            }
            catch (Exception exception)
            {
                log.ErrorException("Exception during server stop", exception);
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
