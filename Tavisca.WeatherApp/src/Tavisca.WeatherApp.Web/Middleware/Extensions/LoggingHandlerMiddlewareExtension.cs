using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tavisca.WeatherApp.Web.Middleware.Extensions
{
    public static class LoggingHandlerMiddlewareExtension
    {
        public static IApplicationBuilder UseLogginerHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LoggingHandlerMiddleware>();
        }
    }
}
