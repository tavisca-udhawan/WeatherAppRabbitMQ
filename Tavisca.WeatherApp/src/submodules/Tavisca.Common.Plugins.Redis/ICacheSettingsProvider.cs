using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.Redis
{
    public interface ICacheSettingsProvider
    {
        Task<RedisSettings> GetCacheSettingsAsync();
    }
}