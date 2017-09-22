using GitCandy.Base;
using System.Globalization;
using Xunit;

namespace GitCandy.Tests
{
    public class LocalizationTests
    {
        [Fact]
        public void TestCultureHelperCaches()
        {
            Assert.Throws<CultureNotFoundException>(() => CultureHelper.NameToCultureInfoCache(""));
            Assert.Throws<CultureNotFoundException>(() => CultureHelper.NameToCultureInfoCache("*"));
            Assert.Throws<CultureNotFoundException>(() => CultureHelper.NameToCultureInfoCache("zz"));

            var a = CultureHelper.NameToCultureInfoCache("en");
            var b = CultureHelper.NameToCultureInfoCache("en-us");
            var c = CultureHelper.NameToCultureInfoCache("EN");
            var d = CultureHelper.NameToCultureInfoCache("EN-US");
            var e = CultureHelper.NameToCultureInfoCache("zh-cn");
            var f = CultureHelper.NameToCultureInfoCache("zh-HANS");

            Assert.Same(a, c);
            Assert.Same(b, d);
            Assert.Equal(a, b);
            Assert.Equal(e, f);
        }

        [Fact]
        public void TestCultureHelperDisplayLink()
        {
            Assert.Equal("English (United Kingdom)", CultureHelper.CultureToDisplayCache("en-gb"));
            Assert.Equal("French (France) - français (France)", CultureHelper.CultureToDisplayCache("fr"));
        }

        [Fact]
        public void EnsureSharedResource()
        {
            Assert.Equal("GitCandy", typeof(SharedResource).Namespace); // Not able in sub namespace
        }
    }
}
