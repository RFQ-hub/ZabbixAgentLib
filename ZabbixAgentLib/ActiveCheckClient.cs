using System;
using System.Net;
using NLog;

namespace Ids.ZabbixAgent
{
    public class ActiveCheckClient : IDisposable
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

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
            
        }
    }
}