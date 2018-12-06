using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tavisca.Platform.Common.WebApi.Middlewares;
using Tavisca.WeatherApp.Core;
using Tavisca.WeatherApp.Model.Interfaces;
using Tavisca.WeatherApp.Models.Interfaces;
using Tavisca.WeatherApp.Service;
using Tavisca.WeatherApp.Service.Data_Contracts.Interfaces;
using Tavisca.WeatherApp.Service.FileSystem;
using Tavisca.WeatherApp.Service.Store;
using Tavisca.WeatherApp.Web.Middleware.Extensions;

namespace Tavisca.WeatherApp.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSingleton<IWeatherAppService, WeatherAppService>();
            services.AddSingleton<IWeatherApp, Tavisca.WeatherApp.Core.WeatherApp>();
            services.AddSingleton<ISessionStore, SessionStore>();
            services.AddSingleton<IFileInit, FileSystem>();
            services.AddSingleton<IFileOperations, ReadFile>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMiddleware<RewindContextStreamMiddleware>();
            app.UseExceptionHandlerInjector();
            app.UseLogginerHandler();
            app.UseMvc();
        }
    }
}
