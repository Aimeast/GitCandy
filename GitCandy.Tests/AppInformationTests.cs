using GitCandy.Base;
using System;
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
        public void CopyrightUpdated()
        {
            Assert.Equal(
                "Copyright © 2013-" + DateTime.Now.Year,
                typeof(AppInformation)
                    .GetTypeInfo()
                    .Assembly
                    .GetCustomAttribute<AssemblyCopyrightAttribute>()
                    .Copyright);
        }
    }
}
