using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using NLog;

namespace Itg.ZabbixAgent
{
    public class PassiveCheckServer : PassiveCheckServerBase
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public PassiveCheckServer(IPEndPoint endpoint) : base(endpoint)
        {
        }

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

        protected override PassiveCheckResult GetValue(string key, string args)
        {
            if (!items.TryGetValue(key, out var getItemMethod))
            {
                return PassiveCheckResult.NotSupported;
            }

            try
            {
                var value = getItemMethod(args);
                var valueString = Convert.ToString(value, CultureInfo.InvariantCulture);
                return new PassiveCheckResult(valueString);
            }
            catch (Exception exception)
            {
                var message = string.Format("Unable to get item '{0}' with args '{1}'", items, args);
                log.ErrorException(message, exception);

                return PassiveCheckResult.NotSupported;
            }
        }
    }
}
