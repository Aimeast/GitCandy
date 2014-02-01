using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

namespace GitCandy
{
    public static class AppInfomation
    {
        public static readonly Version Version = typeof(AppInfomation).Assembly.GetName().Version;
        public static readonly string AssemblyConfiguration;
        public static readonly string BuildingInformation;

        static AppInfomation()
        {
            try
            {
                BuildingInformation = "";

                var assembly = typeof(AppInfomation).Assembly;
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

            var assemblyConfigurationAttr = typeof(AppInfomation).Assembly
                .GetCustomAttributes(typeof(AssemblyConfigurationAttribute), true)
                .FirstOrDefault() as AssemblyConfigurationAttribute;
            AssemblyConfiguration = assemblyConfigurationAttr == null
                ? null
                : assemblyConfigurationAttr.Configuration;
        }

        public static string DateTimeOffsetFormatedNow
        {
            get
            {
                return DateTimeOffset.Now.ToString(DateTimeFormatInfo.InvariantInfo);
            }
        }

        public static string GetAppStartingInfo()
        {
            var sb = new StringBuilder();

            sb.AppendLine("GitCandy Starting....");
            sb.AppendLine("OS Version: " + Environment.OSVersion);
            sb.AppendLine("CLR Version: " + Environment.Version);
            sb.AppendLine("64Bit OS: " + Environment.Is64BitOperatingSystem);
            sb.AppendLine("64Bit Process: " + Environment.Is64BitProcess);
            sb.AppendLine("IIS Version: " + HttpRuntime.IISVersion);
            sb.AppendLine("Target Version: " + HttpRuntime.TargetFramework);
            sb.AppendLine("App Path: " + HttpRuntime.AppDomainAppPath);
            sb.AppendLine("Integrated: " + HttpRuntime.UsingIntegratedPipeline);
            sb.AppendLine("On UNC Share: " + HttpRuntime.IsOnUNCShare);
            sb.AppendLine("GitCandy Version: " + Version);
            sb.AppendLine("Assembly Configuration: " + AssemblyConfiguration);
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