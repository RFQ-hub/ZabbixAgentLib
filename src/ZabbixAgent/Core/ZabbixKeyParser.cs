using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Itg.ZabbixAgent.Core
{
    /// <summary>
    /// Parse the "key[args]" syntax used by zabbix.
    /// </summary>
    internal static class ZabbixKeyParser
    {
        private static readonly Regex keyRegex = new Regex(@"^(?<key>[^\[]*)(\[(?<args>.*)\])?",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public static void Parse(string rawKey, out string key, out string args)
        {
            Debug.Assert(rawKey != null);

            var keyMatch = keyRegex.Match(rawKey);
            if (keyMatch.Success)
            {
                key = keyMatch.Groups["key"].Value;
                args = keyMatch.Groups["args"].Value;
            }
            else
            {
                key = rawKey;
                args = null;
            }
        }
    }
}
