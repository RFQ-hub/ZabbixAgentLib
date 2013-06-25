using System;
using System.Net;

namespace Ids.ZabbixAgent
{
    public class ClientConnectedEventArgs : EventArgs
    {
        public bool DenyConnection { get; set; }

        public IPAddress RemoteAddress { get; private set; }

        public ClientConnectedEventArgs(IPAddress remoteAddress)
        {
            RemoteAddress = remoteAddress;
        }
    }
}