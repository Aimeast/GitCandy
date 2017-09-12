using GitCandy.Data;
using GitCandy.Data.Main;
using Moq;
using Xunit;

namespace GitCandy.Tests
{
    public class DataServiceTests
    {
        [Fact]
        public void EnsureDatabase()
        {
            var accessor = new TestDbAccessor();
            var mockService = new Mock<DataService>(null);
            mockService
                .Setup(x => x.CreateMainDbAccessor())
                .Returns(accessor.CreateMainDbAccessor());

            mockService.Object.EnsureDatabase();
            mockService.Object.EnsureDatabase();

            using (var db = accessor.CreateMainDbAccessor())
            {
                Assert.NotNull(db.First<User>());
                Assert.NotNull(db.First<MigrationHistory>());
            }
        }
    }
}
