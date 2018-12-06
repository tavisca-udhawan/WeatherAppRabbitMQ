using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Crypto;

namespace Tavisca.Common.Plugins.Crypto
{
    public interface ICryptoKeyProviderFactory
    {
        ICryptoKeyProvider GetKeyProvider();
        Task<ICryptoKeyProvider> GetKeyProviderAsync();
    }
}
