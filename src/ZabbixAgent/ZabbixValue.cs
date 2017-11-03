using System;
using System.Globalization;
using Itg.ZabbixAgent.Core;
using JetBrains.Annotations;

namespace Itg.ZabbixAgent
{
    public struct ZabbixValue
    {
        public string Value { get; }
        public bool IsNotSupported => Value == null || Value == ZabbixConstants.NotSupported;

        public static ZabbixValue NotSupported { get; } = default;

        public ZabbixValue([NotNull] string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public static ZabbixValue FromAny<T>([CanBeNull] T value)
        {
            return new ZabbixValue(Convert.ToString(value, CultureInfo.InvariantCulture));
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
