using LiteDB;

namespace GitCandy.Data
{
    public interface IDbAccessor
    {
        LiteRepository CreateMainDbAccessor();
    }
}
