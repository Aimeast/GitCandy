using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace GitCandy.DAL
{
    public class GitCandyDropCreateDatabaseIfModelChanges : DropCreateDatabaseIfModelChanges<GitCandyContext>
    {
        protected override void Seed(GitCandyContext context)
        {
            context.Users.Add(new User() { Email = "admin@GitCandy", Name = "admin", Nickname = "admin", Description = "System administrator", Password = "6BBBDB60C90AD35F944A934B6E83ABDC", PasswordVersion = 1, IsSystemAdministrator = true, CreationDate = System.DateTime.Now });

            base.Seed(context);
        }
    }
}