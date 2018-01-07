using GitCandy.Data;
using GitCandy.Data.Main;
using GitCandy.Security;
using Moq;
using System;
using Xunit;

namespace GitCandy.Tests
{
    public class UserManagerTests
    {
        [Fact]
        public void AddOneUser()
        {
            var manager = CreateUserManager();
            var user = manager.CreateUser("root", "su", "hello", "ab@git", "super user", out _, out _);

            Assert.Equal("root", user.Name);
            Assert.Equal("su", user.Nickname);
            Assert.Equal("ab@git", user.Email);
            Assert.Equal("super user", user.Description);
            Assert.Equal(false, user.IsSystemAdministrator);
        }

        [Fact]
        public void AddDuplicatedUser()
        {
            var manager = CreateUserManager();

            manager.CreateUser("root", "su", "hello", "AB@git", "super user", out var badName, out var badEmail);
            Assert.False(badName);
            Assert.False(badEmail);

            manager.CreateUser("SUPER", "su", "hello", "abxx@git", "super user", out badName, out badEmail);
            Assert.False(badName);
            Assert.False(badEmail);

            manager.CreateUser("root", "su", "hello", "abxx@git", "super user", out badName, out badEmail);
            Assert.True(badName);
            Assert.True(badEmail);

            manager.CreateUser("super", "su", "hello", "abcc@git", "super user", out badName, out badEmail);
            Assert.True(badName);
            Assert.False(badEmail);

            manager.CreateUser("ROOT", "su", "hello", "ab@git", "super user", out badName, out badEmail);
            Assert.True(badName);
            Assert.True(badEmail);
        }

        [Fact]
        public void UserLogin()
        {
            var manager = CreateUserManager();

            manager.CreateUser("root", "su", "hello", "ab@git", "super user", out _, out _);

            Assert.NotNull(manager.Login("root", "hello"));
            Assert.NotNull(manager.Login("ab@GIT", "hello"));
        }

        [Fact]
        public void UpdatePasswordVersion()
        {
            var accessor = new TestDbAccessor();
            var manager = new UserManager(accessor);

            using (var db = accessor.CreateMainDbAccessor())
            {
                db.Insert(new User
                {
                    Name = "admin",
                    Nickname = "su",
                    Email = "ab@git",
                    Description = "super user",
                    CreationDate = DateTime.Now,
                    Password = "5D41402ABC4B2A76B9719D911017C592",
                    PasswordVersion = 1,
                });
            }

            var user = manager.Login("ab@git", "hello");
            Assert.NotNull(user);
            Assert.Equal(PasswordProvider.LastVersion, user.PasswordVersion);
        }

        private UserManager CreateUserManager()
        {
            var accessor = new TestDbAccessor();
            var mockService = new Mock<DataService>(null);
            mockService
                .Setup(x => x.CreateMainDbAccessor())
                .Returns(accessor.CreateMainDbAccessor());

            mockService.Object.EnsureDatabase();
            return mockService.Object.UserManager;
        }
    }
}
