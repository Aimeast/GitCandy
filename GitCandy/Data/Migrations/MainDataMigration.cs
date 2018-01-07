using LiteDB;

namespace GitCandy.Data.Migrations
{
    public abstract class MainDataMigration
    {
        protected LiteRepository _db;

        public MainDataMigration(LiteRepository db)
        {
            _db = db;
        }

        public abstract void Up();
    }
}
