using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using JetBrains.Annotations;

namespace Itg.ZabbixAgent
{
    public class PassiveCheckServerWithStore : PassiveCheckServerBase
    {
        private struct CounterId
        {
            public string Key { get; }
            public string Arguments { get; }

            public CounterId(string key, string arguments)
            {
                Key = key;
                Arguments = arguments;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is CounterId))
                {
                    return false;
                }

                var id = (CounterId)obj;
                return Key == id.Key &&
                       Arguments == id.Arguments;
            }

            public override int GetHashCode()
            {
                var hashCode = -566015957;
                hashCode = hashCode * -1521134295 + base.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Key);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Arguments);
                return hashCode;
            }
        }

        private readonly ConcurrentDictionary<CounterId, string> store = new ConcurrentDictionary<CounterId, string>();

        public PassiveCheckServerWithStore([NotNull] IPEndPoint endpoint)
            : base(endpoint)
        {
        }

        protected override PassiveCheckResult GetValue(string key, string args)
        {
            if (store.TryGetValue(new CounterId(key, args), out var value) && value != null)
            {
                return new PassiveCheckResult(value);
            }

            return PassiveCheckResult.NotSupported;
        }

        public void SetValue<T>([NotNull] string key, [CanBeNull] string arguments, [CanBeNull] T value)
        {
            var counterId = new CounterId(key, arguments);
            if (ReferenceEquals(value, null))
            {
                store.TryRemove(counterId, out _);
            }
            else
            {
                var stringValue = Convert.ToString(value, CultureInfo.InvariantCulture);
                store.AddOrUpdate(counterId, k => stringValue, (k, v) => stringValue);
            }
        }
    }
}
