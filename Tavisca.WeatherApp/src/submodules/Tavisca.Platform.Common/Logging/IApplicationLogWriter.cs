using System.Threading.Tasks;

namespace Tavisca.Platform.Common.Logging
{
    public interface IApplicationLogWriter
    {
        Task WriteAsync(ILog log);
    }

    public interface ILogWriterFactory
    {
        IApplicationLogWriter CreateWriter();
    }
}
