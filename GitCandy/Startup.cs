using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using GitCandy.Logging;
using GitCandy.Base;
using GitCandy.Configuration;

namespace GitCandy
{
    public class Startup
    {
        private IHostingEnvironment _env;

        public Startup(IHostingEnvironment env)
        {
            _env = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var appDataFileProvider = new PhysicalFileProvider(Path.Combine(_env.ContentRootPath, "App_Data"));
            services.AddSingleton(new AppDataStorageSettings { FileProvider = appDataFileProvider });

            services.AddOptions();
            services.ConfigureUserSettings<UserSettings>(appDataFileProvider.GetFileInfo("usersettings.json"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var appDataStorageSettings = app.ApplicationServices.GetService<AppDataStorageSettings>();

            if (!env.IsProduction())
            {
                loggerFactory.AddConsole();
            }
            loggerFactory.AddPlain(appDataStorageSettings.FileProvider, includeScopes: true);

            loggerFactory.CreateLogger<Startup>().LogInformation(AppInformation.GetAppStartingInfo(env));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}
