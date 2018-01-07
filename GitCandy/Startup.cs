﻿using GitCandy.Accessories;
using GitCandy.Base;
using GitCandy.Configuration;
using GitCandy.Data;
using GitCandy.Filters;
using GitCandy.Logging;
using GitCandy.Middlewares;
using GitCandy.Resources;
using GitCandy.Router;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using System.IO;

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
            services.AddDataService(new DataServiceSettings
            {
                MainDbFileInfo = appDataFileProvider.GetFileInfo("GitCandy.db"),
                CacheDbFileInfo = appDataFileProvider.GetFileInfo("Cache.db"),
            });

            services.AddAccessories();
            services.AddFilters();
            services.AddLocalization();
            services
                .AddMvc(options =>
                {
                    options.Filters.AddService<TokenFilter>();
                })
                .AddDataAnnotationsLocalization(options =>
                {
                    options.DataAnnotationLocalizerProvider = (type, factory) =>
                        factory.Create(typeof(SR));
                })
                .AddViewLocalization();

            services.AddGitCandyRouter();
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

            app.UseStaticFiles();

            app.UseProfiler();
            app.UseLightweightLocalization();
            app.UseInjectHttpHeaders();

            app.UseGitCandyRouter();
        }
    }
}
