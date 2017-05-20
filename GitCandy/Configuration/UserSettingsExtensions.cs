using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.IO;

namespace GitCandy.Configuration
{
    public static class UserSettingsExtensions
    {
        public static IServiceCollection ConfigureUserSettings<TOptions>(this IServiceCollection services, IFileInfo fileinfo)
            where TOptions : ConfigurationBase, new()
        {
            services.Configure<TOptions>(options =>
            {
                if (fileinfo.Exists)
                {
                    try
                    {
                        var serializer = new JsonSerializer()
                        {
                            TypeNameHandling = TypeNameHandling.Auto,
                        };
                        using (var reader = new StreamReader(File.Open(fileinfo.PhysicalPath, FileMode.Open)))
                        {
                            serializer.Populate(reader, options);
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                else
                {
                    options.AssignAsDefaultValue();
                }

                options.SettingsFileInfo = fileinfo;
            });

            return services;
        }

        public static void SaveUserSettings<TOptions>(this IOptions<TOptions> options)
            where TOptions : ConfigurationBase, new()
        {
            var settings = options.Value;

            var serializer = new JsonSerializer()
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto,
            };
            using (var writer = new StreamWriter(File.Open(settings.SettingsFileInfo.PhysicalPath, FileMode.Create)))
            {
                serializer.Serialize(writer, settings);
            }
        }
    }
}
