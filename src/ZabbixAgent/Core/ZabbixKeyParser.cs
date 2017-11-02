using System.Diagnostics;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Itg.ZabbixAgent.Core
{
    /// <summary>
    /// Parse the "key[args]" syntax used by zabbix.
    /// </summary>
    internal static class ZabbixKeyParser
    {
        private static readonly Regex keyRegex = new Regex(@"^(?<key>[^\[]*)(\[(?<args>.*)\])?",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public static void Parse([NotNull] string rawKey, [NotNull] out string key, [CanBeNull] out string args)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            Debug.Assert(rawKey != null);

            var keyMatch = keyRegex.Match(rawKey);
            if (keyMatch.Success)
            {
                key = keyMatch.Groups["key"].Value;
                var argsGroup = keyMatch.Groups["args"];
                args = argsGroup.Success ? argsGroup.Value : null;
            }
            else
            {
                key = rawKey;
                args = null;
            }
        }
    }
}
