using GitCandy.Data.Main;
using LiteDB;

namespace GitCandy.Data.Migrations
{
    public class _0001_Initial : MainDataMigration
    {
        public _0001_Initial(LiteRepository db)
            : base(db)
        {
        }

        public override void Up()
        {
            _db.Database.GetCollection<User>().EnsureIndex(x => x.Name, $"LOWER($.Name)", true);
            _db.Database.GetCollection<User>().EnsureIndex(x => x.Email, $"LOWER($.Email)", true);
        }
    }
}
