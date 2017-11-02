using System;
using System.Net;
using JetBrains.Annotations;

namespace Itg.ZabbixAgent
{
    public class ClientConnectedEventArgs : EventArgs
    {
        public bool DenyConnection { get; set; }

        [NotNull]
        public IPAddress RemoteAddress { get; }

        public ClientConnectedEventArgs([NotNull] IPAddress remoteAddress)
        {
            RemoteAddress = remoteAddress;
        }
    }
}
