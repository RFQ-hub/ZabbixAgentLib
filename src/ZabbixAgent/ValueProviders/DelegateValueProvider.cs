using System;
using System.Collections.Generic;
using System.Globalization;
using Itg.ZabbixAgent.Core;
using JetBrains.Annotations;
using NLog;

namespace Itg.ZabbixAgent.ValueProviders
{
    public class DelegateValueProvider : IZabbixValueProvider
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private delegate object GetItemMethod(string args);

        public delegate T TypedGetItemMethodWithArgs<out T>([CanBeNull] string args);
        public delegate T TypedGetItemMethod<out T>();

        private readonly Dictionary<string, GetItemMethod> items = new Dictionary<string, GetItemMethod>();

        public IDisposable AddItem<T>([NotNull] string item, [NotNull] TypedGetItemMethodWithArgs<T> getItem)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (getItem == null)
            {
                throw new ArgumentNullException(nameof(getItem));
            }

            items.Add(item, args => getItem(args));
            return new Disposable(() => items.Remove(item));
        }

        public IDisposable AddItem<T>(string item, TypedGetItemMethod<T> getItem)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (getItem == null)
            {
                throw new ArgumentNullException(nameof(getItem));
            }

            items.Add(item, args => getItem());
            return new Disposable(() => items.Remove(item));
        }

        public ZabbixValue GetValue(string key, string args)
        {
            if (!items.TryGetValue(key, out var getItemMethod))
            {
                return ZabbixValue.NotSupported;
            }

            try
            {
                var value = getItemMethod(args);
                var valueString = Convert.ToString(value, CultureInfo.InvariantCulture);
                return new ZabbixValue(valueString);
            }
            catch (Exception exception)
            {
                log.ErrorException(
                    $"Unable to get item '{items}' with args '{args}'",
                    exception);

                return ZabbixValue.NotSupported;
            }
        }
    }
}
