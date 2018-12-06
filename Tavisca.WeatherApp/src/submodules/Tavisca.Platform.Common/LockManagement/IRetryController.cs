namespace Tavisca.Platform.Common.LockManagement
{
    public interface IRetryController
    {
        int? GetNextRetryInterval();
    }
}
