using Microsoft.AspNetCore.Hosting;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace GitCandy.Base
{
    public static class AppInformation
    {
        public static readonly Version Version;
        public static readonly string BuildingInformation;
        public static readonly DateTimeOffset StartingTime = DateTimeOffset.Now;

        static AppInformation()
        {
            BuildingInformation = "";
            var assembly = typeof(AppInformation).GetTypeInfo().Assembly;
            Version = assembly.GetName().Version;
            try
            {
                var name = assembly.GetManifestResourceNames().Single(s => s.EndsWith(".Information"));
                using (var stream = assembly.GetManifestResourceStream(name))
                using (var reader = new StreamReader(stream))
                {
                    BuildingInformation = reader.ReadToEnd().Trim();
                }
            }
            catch
            {
            }
        }

        public static string GetAppStartingInfo(IHostingEnvironment env)
        {
            var sb = new StringBuilder();

            sb.AppendLine("GitCandy Starting....");
            sb.AppendLine("Process Architecture: " + RuntimeInformation.ProcessArchitecture);
            sb.AppendLine("Processor Count: " + Environment.ProcessorCount);
            sb.AppendLine("OS Architecture: " + RuntimeInformation.OSArchitecture);
            sb.AppendLine("OS Description: " + RuntimeInformation.OSDescription);
            sb.AppendLine("Framework Description: " + RuntimeInformation.FrameworkDescription);
            sb.AppendLine("Machine Name: " + Environment.MachineName);
            sb.AppendLine("Application Name: " + env.ApplicationName);
            sb.AppendLine("Environment Name: " + env.EnvironmentName);
            sb.AppendLine("Content Root Path: " + env.ContentRootPath);
            sb.AppendLine("Web Root Path: " + env.WebRootPath);
            sb.AppendLine("GitCandy Version: " + Version);

            sb.AppendLine(BuildingInformation);

            return sb.ToString();
        }

        public static string GetAppStartedInfo()
        {
            return "GitCandy Started.";
        }

        public static string GetAppEndInfo()
        {
            return "GitCandy End.";
        }
    }
}
