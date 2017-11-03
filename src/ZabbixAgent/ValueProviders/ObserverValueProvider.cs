using System;
using NLog;

namespace Itg.ZabbixAgent.ValueProviders
{
    public class ObserverValueProvider : IObserver<ZabbixValueChanged>, IZabbixValueProvider
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly StoredValueProvider storedProvider = new StoredValueProvider();

        public void OnCompleted()
        {
            log.Debug("Attached observable has completed.");
        }

        public void OnError(Exception error)
        {
            log.ErrorException("Attached observable has errored.", error);
        }

        public void OnNext(ZabbixValueChanged value)
        {
            storedProvider.SetValue(value.Key, value.Arguments, value.Value);
        }

        public ZabbixValue GetValue(string key, string args)
        {
            return storedProvider.GetValue(key, args);
        }
    }
}
