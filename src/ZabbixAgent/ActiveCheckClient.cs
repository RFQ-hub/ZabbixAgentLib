using System;
using System.Net;
using NLog;

namespace Itg.ZabbixAgent
{
    // TODO
    internal class ActiveCheckClient : IDisposable
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public ActiveCheckClient(IPEndPoint endpoint)
        {
            UpdateMonitoredItems();
        }

        private void UpdateMonitoredItems()
        {
            
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
                    log.Error("Exception during server stop", exception);
                }
            }
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
        }

        public void Stop()
        {
            
        }
    }
}
