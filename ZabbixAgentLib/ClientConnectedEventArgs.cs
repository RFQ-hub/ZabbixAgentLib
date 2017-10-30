using System;
using System.Net;

namespace Itg.ZabbixAgentLib
{
    public class ClientConnectedEventArgs : EventArgs
    {
        public bool DenyConnection { get; set; }

        public IPAddress RemoteAddress { get; }

        public ClientConnectedEventArgs(IPAddress remoteAddress)
        {
            RemoteAddress = remoteAddress;
        }
    }
}