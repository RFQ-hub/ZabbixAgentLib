using System;
using JetBrains.Annotations;

namespace Itg.ZabbixAgent
{
    public struct PassiveCheckResult
    {
        public string Value { get; }
        public bool IsNotSupported => Value == null;

        public static PassiveCheckResult NotSupported { get; } = default;

        public PassiveCheckResult([NotNull] string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}
