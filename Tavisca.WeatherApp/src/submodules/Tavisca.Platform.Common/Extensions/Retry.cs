using System;
using System.Threading.Tasks;
using Tavisca.Platform.Common;

namespace Tavisca.Platform.Common
{
    public static class Retry
    {
        public static void ExecuteWithFaultSuppression(Action action, int count = 3)
        {
            do
            {
                try
                {
                    action();
                    return;
                }
                catch (Exception ex)
                {
                    ExceptionPolicy.HandleException(ex, "logonly");
                    count--;
                }
            } while (count > 0);
        }

        public static async Task ExecuteWithFaultSuppressionAsync(Func<Task> actionAsync, int count = 3)
        {
            do
            {
                try
                {
                    await actionAsync();
                    return;
                }
                catch (Exception ex)
                {
                    ExceptionPolicy.HandleException(ex, "logonly");
                    count--;
                }
            } while (count > 0);
        }
    }
}
