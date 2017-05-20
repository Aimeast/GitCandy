using GitCandy.Configuration;
using GitCandy.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace GitCandy.Tests
{
    public class ConfigurationTests
    {
        [Fact]
        public void LoadUserConfigAsDefaultAndSave()
        {
            var fileProvider = FileHelper.GetFileProvider(nameof(ConfigurationTests));
            var filename = DateTime.Now.Ticks + ".json";

            var services1 = new ServiceCollection()
                .AddOptions()
                .ConfigureUserSettings<FakeUserSetting>(fileProvider.GetFileInfo(filename));
            var provider1 = services1.BuildServiceProvider();
            var option1 = provider1.GetService<IOptions<FakeUserSetting>>();

            // assign to default value
            Assert.Equal(true, option1.Value.TestBoolean);
            Assert.Equal(2, option1.Value.TestInt);
            Assert.Null(option1.Value.TestString);

            var hostkey1 = option1.Value.TestKeys.Single() as FakeHostKey;
            Assert.NotNull(hostkey1);
            Assert.Equal(new byte[] { 1, 2, 3 }, hostkey1.A);
            Assert.Equal(4, hostkey1.B);

            option1.Value.TestBoolean = false;
            option1.Value.TestInt = 4;
            option1.Value.TestString = "5";
            hostkey1.B = 6;

            option1.SaveUserSettings();

            var services2 = new ServiceCollection()
                .AddOptions()
                .ConfigureUserSettings<FakeUserSetting>(fileProvider.GetFileInfo(filename));
            var provider2 = services2.BuildServiceProvider();
            var option2 = provider2.GetService<IOptions<FakeUserSetting>>();

            // load saved value
            Assert.Equal(false, option2.Value.TestBoolean);
            Assert.Equal(4, option2.Value.TestInt);
            Assert.Equal("5", option2.Value.TestString);

            var hostkey2 = option2.Value.TestKeys.Single() as FakeHostKey;
            Assert.NotNull(hostkey1);
            Assert.Equal(new byte[] { 1, 2, 3 }, hostkey1.A);
            Assert.Equal(6, hostkey1.B);
        }

        class FakeUserSetting : ConfigurationBase
        {
            [RecommendedValue(true)]
            public bool TestBoolean { get; set; }

            [RecommendedValue(1, defaultValue: 2)]
            public int TestInt { get; set; }

            public string TestString { get; set; }

            [FakeReslover]
            public List<IHostKey> TestKeys { get; set; }
        }

        class FakeResloverAttribute : RecommendedValueResloverAttribute
        {
            public override object GetValue()
            {
                return new List<IHostKey> {
                    new FakeHostKey {
                        A = new byte[] { 1, 2, 3 },
                        B = 4,
                    }
                };
            }
        }

        class FakeHostKey : IHostKey
        {
            public byte[] A;
            public int B;
        }
    }
}
