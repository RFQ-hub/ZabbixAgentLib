using Itg.ZabbixAgent.Core;
using NFluent;
using Xunit;

namespace Itg.ZabbixAgent.Tests.Core
{
    public class ZabbixKeyParserTests
    {
        [Fact]
        public void Parse_simple_works()
        {
            ZabbixKeyParser.Parse("foo", out var key, out var args);
            Check.That(key).IsEqualTo("foo");
            Check.That(args).IsNull();
        }

        [Fact]
        public void Parse_with_args_works()
        {
            ZabbixKeyParser.Parse("foo[bar]", out var key, out var args);
            Check.That(key).IsEqualTo("foo");
            Check.That(args).IsEqualTo("bar");
        }

        [Fact]
        public void Parse_with_broken_brackets_works()
        {
            ZabbixKeyParser.Parse("foo[bar]baz", out var key, out var args);
            Check.That(key).IsEqualTo("foo[bar]baz");
            Check.That(args).IsNull();
        }

        [Fact]
        public void Parse_with_inner_brackets_works()
        {
            ZabbixKeyParser.Parse("foo[bar]]", out var key, out var args);
            Check.That(key).IsEqualTo("foo");
            Check.That(args).IsEqualTo("bar]");
        }
    }
}
