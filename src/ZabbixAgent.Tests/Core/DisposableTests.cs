using System.Threading;
using Itg.ZabbixAgent.Core;
using NFluent;
using Xunit;

namespace Itg.ZabbixAgent.Tests
{
    public class DisposableTests
    {
        [Fact]
        public void Called_OnDispose()
        {
            var count = 0;
            void Inc() => Interlocked.Increment(ref count);
            var disposable = new Disposable(Inc);

            Check.That(count).IsEqualTo(0);

            disposable.Dispose();
            Check.That(count).IsEqualTo(1);
        }
    }
}