using LiteDB;

namespace GitCandy.Data.Main.Migrations
{
    public class _0001_Initial : MainDataMigration
    {
        public _0001_Initial(LiteRepository db)
            : base(db)
        {
        }

        public override void Up()
        {
            _db.Database.GetCollection<User>().EnsureIndex(x => x.Name, true, $"LOWER($.Name)");
            _db.Database.GetCollection<User>().EnsureIndex(x => x.Email, true, $"LOWER($.Email)");
        }
    }
}
