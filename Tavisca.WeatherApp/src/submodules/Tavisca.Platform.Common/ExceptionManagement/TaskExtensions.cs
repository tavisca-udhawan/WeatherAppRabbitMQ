using System.Threading.Tasks;

namespace Tavisca.Platform.Common.ExceptionManagement
{
    public static class TaskExtensions
    {
        public static Task WithErrorHandling(this Task task, string policy)
        {
            return task.ContinueWith(t =>
                                     {
                                         if (t.IsFaulted)
                                             ExceptionPolicy.HandleException(t.Exception, policy);
                                     });
        }

        public static Task<T> WithErrorHandling<T>(this Task<T> task, string policy)
        {
            return task.ContinueWith(t =>
                                     {
                                         if (t.IsFaulted)
                                             ExceptionPolicy.HandleException(t.Exception, policy);
                                         return t.Result;
                                     });
        }
    }
}
