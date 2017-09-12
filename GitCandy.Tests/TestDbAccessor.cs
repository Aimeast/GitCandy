using GitCandy.Data;
using LiteDB;
using System.IO;

namespace GitCandy.Tests
{
    public class TestDbAccessor : IDbAccessor
    {
        MemoryStream _ms = new MemoryStream(64 * 1024);

        public LiteRepository CreateMainDbAccessor()
        {
            return new LiteRepository(_ms);
        }
    }
}
