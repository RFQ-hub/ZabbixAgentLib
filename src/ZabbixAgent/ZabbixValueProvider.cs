using JetBrains.Annotations;

namespace Itg.ZabbixAgent
{
    public delegate ZabbixValue ZabbixValueProvider([NotNull] string key, [CanBeNull] string args);
}
