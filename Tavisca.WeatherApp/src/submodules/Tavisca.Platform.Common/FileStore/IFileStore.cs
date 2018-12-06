using System;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.FileStore
{
    public interface IFileStore : IDisposable
    {
        void Add(string key, string path, byte[] bytes);
        Task AddAsync(string key, string path, byte[] bytes);
        byte[] Get(string key, string path);
        Task<byte[]> GetAsync(string key, string path);
    }
}
