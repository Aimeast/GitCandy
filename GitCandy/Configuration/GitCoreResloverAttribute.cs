using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GitCandy.Configuration
{
    public class GitCoreResloverAttribute : RecommendedValueResloverAttribute
    {
        public override object GetValue()
        {
            var list = new List<string>();
            var variable = Environment.GetEnvironmentVariable("path");
            if (variable != null)
                list.AddRange(variable.Split(';'));

            list.Add(Environment.GetEnvironmentVariable("ProgramW6432"));
            list.Add(Environment.GetEnvironmentVariable("ProgramFiles"));

            foreach (var drive in Environment.GetLogicalDrives())
            {
                list.Add(drive + @"Program Files\Git");
                list.Add(drive + @"Program Files (x86)\Git");
                list.Add(drive + @"Program Files\PortableGit");
                list.Add(drive + @"Program Files (x86)\PortableGit");
                list.Add(drive + @"PortableGit");
            }

            list = list.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            foreach (var path in list)
            {
                var ret = SearchPath(path);
                if (ret != null)
                    return ret;
            }

            return "";
        }

        private string SearchPath(string path)
        {
            var patterns = new[] {
                @"..\libexec\git-core", // git 1.x
                @"libexec\git-core", // git 1.x
                @"..\mingw64\libexec\git-core", // git 2.x
                @"mingw64\libexec\git-core", // git 2.x
            };
            foreach (var pattern in patterns)
            {
                var fullpath = new DirectoryInfo(Path.Combine(path, pattern)).FullName;
                if (File.Exists(Path.Combine(fullpath, "git.exe"))
                    && File.Exists(Path.Combine(fullpath, "git-receive-pack.exe"))
                    && File.Exists(Path.Combine(fullpath, "git-upload-archive.exe"))
                    && File.Exists(Path.Combine(fullpath, "git-upload-pack.exe")))
                    return fullpath;
            }
            return null;
        }
    }
}
