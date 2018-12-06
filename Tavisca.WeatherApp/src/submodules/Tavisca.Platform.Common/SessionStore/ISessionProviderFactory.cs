namespace Tavisca.Common.Plugins.SessionStore
{
    public interface ISessionProviderFactory
    {
        ISessionDataProvider GetSessionProvider();
    }
}