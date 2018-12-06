using System.Threading.Tasks;
using System.Collections.Generic;

namespace Tavisca.Platform.Common.Configurations
{
    public interface ISensitiveDataProvider
    {
        Task<Dictionary<string, string>> GetValuesAsync(List<string> keys, bool decryptionRequired);
    }
}