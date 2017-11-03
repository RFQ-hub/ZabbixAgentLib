using System.Collections.Concurrent;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Itg.ZabbixAgent.ValueProviders
{
    public class StoredValueProvider: IZabbixValueProvider
    {
        private struct CounterId
        {
            private readonly string key;
            private readonly string arguments;

            public CounterId(string key, string arguments)
            {
                this.key = key;
                this.arguments = arguments;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is CounterId))
                {
                    return false;
                }

                var id = (CounterId) obj;
                return key == id.key &&
                       arguments == id.arguments;
            }

            public override int GetHashCode()
            {
                var hashCode = -566015957;
                hashCode = hashCode * -1521134295 + base.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(key);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(arguments);
                return hashCode;
            }
        }

        private readonly ConcurrentDictionary<CounterId, string> store = new ConcurrentDictionary<CounterId, string>();

        public ZabbixValue GetValue(string key, string args)
        {
            if (store.TryGetValue(new CounterId(key, args), out var value) && value != null)
            {
                return new ZabbixValue(value);
            }

            return ZabbixValue.NotSupported;
        }

        public void SetValue([NotNull] string key, [CanBeNull] string arguments, ZabbixValue value)
        {
            var counterId = new CounterId(key, arguments);
            if (value.IsNotSupported)
            {
                store.TryRemove(counterId, out _);
            }
            else
            {
                store.AddOrUpdate(counterId, k => value.Value, (k, v) => value.Value);
            }
        }

        public void SetValue<T>([NotNull] string key, [CanBeNull] string arguments, [CanBeNull] T value)
        {
            SetValue(key, arguments, ZabbixValue.FromAny(value));
        }

        public void SetValue<T>([NotNull] string key, [CanBeNull] T value)
        {
            SetValue(key, null, ZabbixValue.FromAny(value));
        }
    }
}
