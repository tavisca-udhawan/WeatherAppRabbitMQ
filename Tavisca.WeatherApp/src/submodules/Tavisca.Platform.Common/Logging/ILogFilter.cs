namespace Tavisca.Platform.Common.Logging
{
    public interface ILogFilter
    {
        ILog Apply(ILog log);
    }    
}
