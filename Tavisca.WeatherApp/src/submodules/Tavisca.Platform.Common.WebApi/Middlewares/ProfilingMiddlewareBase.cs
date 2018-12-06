using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Logging;
using Tavisca.Platform.Common.Profiling;

namespace Tavisca.Platform.Common.WebApi.Middlewares
{
    public abstract class ProfilingMiddlewareBase
    {
        protected ProfilingMiddlewareBase(RequestDelegate next)
        {
            _next = next;
        }

        private readonly RequestDelegate _next;

        public async Task Invoke(HttpContext httpContext)
        {
            if (IsProfilingEnabled())
            {
                using (var profileContext = new ProfileContext(httpContext.Request.Path.Value, true))
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    profileContext.OnDispose += async profileData =>
                    {
                        stopwatch.Stop();
                        await LogProfileData(httpContext.Request.Path.Value, profileData, stopwatch);
                    };
                    await _next.Invoke(httpContext);
                }
            }
            else
            {
                await _next.Invoke(httpContext);
            }
        }
        public abstract bool IsProfilingEnabled();

        public abstract Task<ApiLog> GetProfileLog(ProfileTreeNode profileData);

        public async Task LogProfileData(string requestUri, ProfileTreeNode profileData, Stopwatch watch)
        {
            if (profileData == null) return;

            ApiLog profileLog = await GetProfileLog(profileData);
            profileLog.Url = requestUri;
            profileLog.LogTime = DateTime.UtcNow.Subtract(watch.Elapsed);

            profileLog.TimeTakenInMs = watch.ElapsedMilliseconds;
            await Logger.WriteLogAsync(profileLog);

        }

    }
}
