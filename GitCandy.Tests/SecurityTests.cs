using GitCandy.Security;
using System.Linq;
using System.Reflection;
using Xunit;

namespace GitCandy.Tests
{
    public class SecurityTests
    {
        [Fact]
        public void ContractsOfAllProviders()
        {
            var providerType = typeof(PasswordProvider);
            var types = providerType
                .GetTypeInfo()
                .Assembly
                .GetTypes()
                .Where(t => t != providerType && providerType.IsAssignableFrom(t))
                .ToArray();

            foreach (var t in types)
            {
                Assert.Equal(providerType.Namespace, t.Namespace);
                Assert.Single(t.GetTypeInfo().GetCustomAttributes(typeof(PasswordProviderVersionAttribute), false));
                Assert.Single(t.GetTypeInfo().GetConstructors());
                Assert.True(t.GetTypeInfo().IsSealed);
            }
        }

        [Fact]
        public void PeekedRightVersion()
        {
            Assert.Equal(1, PasswordProvider.Peek(1).Version);
            Assert.Equal(2, PasswordProvider.Peek(2).Version);
            Assert.Equal(2, PasswordProvider.LastVersion); // make sure all versions checked
        }

        [Theory]
        [InlineData(1, 0L, "admin", "gitcandy", "6BBBDB60C90AD35F944A934B6E83ABDC")]
        [InlineData(1, 1L, "foo", "gitcandy", "6BBBDB60C90AD35F944A934B6E83ABDC")]
        [InlineData(2, 1L, "admin", "gitcandy", "87F33E9001A7D55BBBAEC8DCC0959A37")]
        [InlineData(2, 2L, "foo", "foobar", "CF9CE2B84D5A03B4EF1627947898CD8F")]
        public void ComputePassword(int version, long uid, string username, string password, string expected)
        {
            Assert.Equal(expected, PasswordProvider.Peek(version).Compute(uid, username, password));
        }

        [Fact]
        public void RandomPassword()
        {
            Assert.NotNull(PasswordProvider.Peek().GenerateRandomPassword());
            Assert.Equal(10, PasswordProvider.Peek().GenerateRandomPassword(10).Length);
        }
    }
}
