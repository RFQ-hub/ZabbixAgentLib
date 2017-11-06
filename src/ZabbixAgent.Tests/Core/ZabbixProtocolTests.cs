using System.IO;
using System.Text;
using Itg.ZabbixAgent.Core;
using NFluent;
using Xunit;

namespace Itg.ZabbixAgent.Tests
{
    public class ZabbixProtocolTests
    {
        [Theory]
        [InlineData("", null, "ZBXD\x01\x00\x00\x00\x00\x00\x00\x00\x00")]
        [InlineData("foo", null, "ZBXD\x01\x03\x00\x00\x00\x00\x00\x00\x0000foo")]
        [InlineData("foo", "bar", "ZBXD\x01\x03\x00\x00\x00\x00\x00\x00\x0000foo\0bar")]
        public void WriteWithHeader_theory(string value, string error, string expected)
        {
            using (var stream = new MemoryStream())
            {
                ZabbixProtocol.WriteWithHeader(stream, value, error);
                var actualBytes = stream.ToArray();
                var expectedBytes = Encoding.ASCII.GetBytes(expected);
                Check.That(actualBytes).ContainsExactly(expectedBytes);
            }
        }
    }
}
