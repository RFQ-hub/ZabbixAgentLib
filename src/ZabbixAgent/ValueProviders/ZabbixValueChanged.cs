using JetBrains.Annotations;

namespace Itg.ZabbixAgent.ValueProviders
{
    public struct ZabbixValueChanged
    {
        [NotNull]
        public string Key { get; }

        [CanBeNull]
        public string Arguments { get; }

        public ZabbixValue Value { get; }

        private ZabbixValueChanged([NotNull] string key, [CanBeNull] string arguments, ZabbixValue value)
        {
            Key = key;
            Arguments = arguments;
            Value = value;
        }

        public static ZabbixValueChanged Create<T>([NotNull] string key, [CanBeNull] string arguments, [CanBeNull] T value)
        {
            return new ZabbixValueChanged(key,arguments, ZabbixValue.FromAny(value));
        }

        public static ZabbixValueChanged Create<T>([NotNull] string key, [CanBeNull] T value)
        {
            return new ZabbixValueChanged(key, null, ZabbixValue.FromAny(value));
        }
    }
}