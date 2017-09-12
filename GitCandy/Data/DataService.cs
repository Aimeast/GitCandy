using GitCandy.Data.Main;
using GitCandy.Data.Main.Migrations;
using LiteDB;
using System;
using System.Linq;
using System.Reflection;

namespace GitCandy.Data
{
    public class DataService : IDbAccessor
    {
        private readonly DataServiceSettings _settings;

        public DataService(DataServiceSettings settings)
        {
            _settings = settings;
            UserManager = new UserManager(this);
        }

        public UserManager UserManager { get; }

        public void EnsureDatabase()
        {
            using (var db = CreateMainDbAccessor())
            {
                var histories = db
                    .Query<MigrationHistory>()
                    .Where(x => x.Success)
                    .ToArray();

                var isNewDb = !histories.Any();
                var migrations = GetMigrations(isNewDb);

                var history = new MigrationHistory
                {
                    MigrationDateTime = DateTime.Now,
                    Success = false,
                };

                for (var i = 0; i < migrations.Length; i++)
                {
                    var migration = migrations[i];

                    if (histories.Any(x => x.Version == migration.Version))
                    {
                        continue;
                    }

                    history.Version = migration.Version;
                    try
                    {
                        var instance = (MainDataMigration)migration.Type
                            .GetConstructor(new[] { typeof(LiteRepository) })
                            .Invoke(new[] { db });

                        instance.Up();

                        history.Success = true;
                    }
                    catch (Exception ex)
                    {
                        history.Message = ex.ToString();
                    }

                    db.Insert(history);
                }

                if (isNewDb)
                {
                    var user = UserManager.AddUser("admin", "admin", "gitcandy", "admin@GitCandy", "", out _, out _);
                    user.IsSystemAdministrator = true;
                    db.Update(user);
                }
            }
        }

        public virtual LiteRepository CreateMainDbAccessor()
        {
            return new LiteRepository(_settings.MainDbFileInfo.PhysicalPath);
        }

        private MigrationPair[] GetMigrations(bool isNewDb)
        {
            var migrationType = typeof(MainDataMigration);
            var migrations = migrationType
                .GetTypeInfo()
                .Assembly
                .GetTypes()
                .Where(t => t != migrationType && migrationType.IsAssignableFrom(t))
                .Select(t =>
                {
                    var version = 0;
                    try
                    {
                        version = int.Parse(t.Name.Split('_')[1]);
                    }
                    catch
                    {
                        version = 0;
                    }
                    return new MigrationPair
                    {
                        Version = version,
                        Type = t,
                    };
                })
                .Where(x => x.Version > 0)
                .OrderBy(x => x.Version);
            if (isNewDb)
                migrations.TakeLast(1);

            return migrations.ToArray();
        }

        class MigrationPair
        {
            public int Version;
            public Type Type;
        }
    }
}
