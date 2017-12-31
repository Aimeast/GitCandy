using GitCandy.Base;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace GitCandy.Tests
{
    public class AppInformationTests
    {
        [Fact]
        public void HasEmbeddedInformation()
        {
            Assert.Single(
                typeof(AppInformation)
                    .GetTypeInfo()
                    .Assembly
                    .GetManifestResourceNames(),
                s => s.EndsWith(".Information"));
        }

        [Fact]
        public void NoInvalidSymbol()
        {
            var assembly = typeof(AppInformation).GetTypeInfo().Assembly;
            using (var stream = assembly.GetManifestResourceStream("GitCandy.Properties.Information"))
            using (var reader = new StreamReader(stream))
            {
                var info = reader.ReadToEnd();
                Assert.DoesNotContain("-->", info); // HTML comment end tag
            }
        }

        [Fact]
        public void CopyrightUpdated()
        {
            Assert.Equal(
                "Copyright © 2013-" + DateTime.Now.Year,
                typeof(AppInformation)
                    .GetTypeInfo()
                    .Assembly
                    .GetCustomAttribute<AssemblyCopyrightAttribute>()
                    .Copyright);

            var path = Directory.GetCurrentDirectory();
            path = path.Substring(0, path.LastIndexOf("bin"));
            var content = File.ReadAllText(new DirectoryInfo(path).Parent.GetFiles("LICENSE.md").First().FullName);
            Assert.Contains("Copyright (c) 2013-" + DateTime.Now.Year + " Aimeast", content);
        }
    }
}
