using JetBrains.Annotations;

namespace Itg.ZabbixAgent
{
    public interface IZabbixValueProvider
    {
        ZabbixValue GetValue([NotNull] string key, [CanBeNull] string args);
    }
}
