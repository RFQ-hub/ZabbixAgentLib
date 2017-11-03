using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NLog;

namespace Itg.ZabbixAgent
{
    public static class ZabbixValueProviderExtensions
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static ZabbixValueProvider Combine([NotNull] this IEnumerable<ZabbixValueProvider> valueProviders)
        {
            if (valueProviders == null)
            {
                throw new ArgumentNullException(nameof(valueProviders));
            }

            var valueProvidersCollection = valueProviders as ICollection<ZabbixValueProvider> ?? valueProviders.ToList();

            return (key, args) =>
            {
                foreach (var valueProvider in valueProvidersCollection)
                {
                    try
                    {
                        var result = valueProvider(key, args);
                        if (!result.IsNotSupported)
                        {
                            return result;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.ErrorException($"Value provider failed for key '{key}' with args '{args}'", ex);
                    }
                }

                return ZabbixValue.NotSupported;
            };
        }

        public static ZabbixValueProvider Combine([NotNull] this IEnumerable<IZabbixValueProvider> valueProviders)
        {
            if (valueProviders == null)
            {
                throw new ArgumentNullException(nameof(valueProviders));
            }

            return valueProviders.Select(p => (ZabbixValueProvider)p.GetValue).Combine();
        }
    }
}
