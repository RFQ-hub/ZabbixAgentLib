using System;
using JetBrains.Annotations;

namespace Itg.ZabbixAgent.Core
{
    public class Disposable : IDisposable
    {
        private readonly Action action;

        public Disposable([NotNull] Action action)
        {
            this.action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public void Dispose()
        {
            action();
        }
    }
}
