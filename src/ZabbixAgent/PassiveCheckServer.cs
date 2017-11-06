using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Itg.ZabbixAgent.Core;
using JetBrains.Annotations;
using NLog;

namespace Itg.ZabbixAgent
{
    public class PassiveCheckServer : IDisposable
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        [NotNull]
        private readonly ZabbixValueProvider valueProvider;
        private readonly TcpListener tcpListener;

        public PassiveCheckServer([NotNull] IPEndPoint endpoint, [NotNull] ZabbixValueProvider valueProvider)
        {
            if (endpoint == null)
            {
                throw new ArgumentNullException(nameof(endpoint));
            }

            this.valueProvider = valueProvider ?? throw new ArgumentNullException(nameof(valueProvider));

            tcpListener = new TcpListener(endpoint);
        }

        public PassiveCheckServer([NotNull] IPEndPoint endpoint,
            [NotNull] IZabbixValueProvider valueProvider)
            : this(endpoint, valueProvider.GetValue)
        {
        }

        public PassiveCheckServer([NotNull] IPEndPoint endpoint,
            [NotNull] IEnumerable<ZabbixValueProvider> valueProviders)
            : this(endpoint, valueProviders.Combine())
        {
        }

        public PassiveCheckServer([NotNull] IPEndPoint endpoint,
            [NotNull] IEnumerable<IZabbixValueProvider> valueProviders)
            : this(endpoint, valueProviders.Combine())
        {
        }

        public PassiveCheckServer([NotNull] IPEndPoint endpoint,
            [NotNull] params ZabbixValueProvider[] valueProviders)
            : this(endpoint, valueProviders.Combine())
        {
        }

        public PassiveCheckServer([NotNull] IPEndPoint endpoint,
            [NotNull] params IZabbixValueProvider[] valueProviders)
            : this(endpoint, valueProviders.Combine())
        {
        }

        public bool IsStarted { get; private set; }

        public void Start()
        {
            if (IsStarted)
            {
                return;
            }

            log.Info("Starting passive server on {0}", tcpListener.LocalEndpoint);
            tcpListener.Start();
            IsStarted = true;
            BeginAcceptTcpClient();
        }

        private void BeginAcceptTcpClient()
        {
            if (!IsStarted)
            {
                return;
            }

            log.Trace("Listening for new connections");
            tcpListener.BeginAcceptTcpClient(OnClientConnected, null);
        }

        public event EventHandler<ClientConnectedEventArgs> ClientConnected;

        private void RaiseClientConnected([NotNull] IPAddress address, out bool denyConnection)
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

        private void OnClientConnected([NotNull] IAsyncResult asyncResult)
        {
            try
            {
                // Accept the connection
                TcpClient tcpClient;
                try
                {
                    tcpClient = tcpListener.EndAcceptTcpClient(asyncResult);
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

        private void HandleClient([NotNull] TcpClient tcpClient)
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

        private void ReadKeyAndWriteAnswer([NotNull] NetworkStream stream)
        {
            var streamReader = new StreamReader(stream);
            var key = streamReader.ReadLine();
            if (key != null)
            {
                log.Trace("Received: {0}", key);

                var valueString = GetItemStringValue(key);

                log.Trace("Answering: {0}", valueString);

                ZabbixProtocol.WriteWithHeader(stream, valueString, null);
            }
        }

        private string GetItemStringValue([NotNull] string key)
        {
            ZabbixValue value;

            ZabbixKeyParser.Parse(key, out key, out var args);

            if (key == "agent.ping")
            {
                return "1";
            }

            try
            {
                value = valueProvider(key, args);
            }
            catch (Exception exception)
            {
                log.ErrorException($"Unable to get item '{key}'", exception);

                value = ZabbixValue.NotSupported;
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
            if (IsStarted)
            {
                log.Debug("Stopping server on {0}", tcpListener.LocalEndpoint);
                tcpListener.Stop();

                log.Info("Stopped server on {0}", tcpListener.LocalEndpoint);
                IsStarted = false;
            }
        }
    }
}
