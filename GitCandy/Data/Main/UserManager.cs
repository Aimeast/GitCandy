using GitCandy.Security;
using System;

namespace GitCandy.Data.Main
{
    public class UserManager
    {
        private readonly IDbAccessor _dbAccessor;

        public UserManager(IDbAccessor dbAccessor)
        {
            _dbAccessor = dbAccessor;
        }

        public User AddUser(string name, string nickname, string password, string email, string description, out bool badName, out bool badEmail)
        {
            var lowercaseName = name.ToLower();
            var lowercaseEmail = email.ToLower();

            badName = false;
            badEmail = false;
            using (var db = _dbAccessor.CreateMainDbAccessor())
            {
                var has = db.Query<User>()
                    .Where(x => x.Name == lowercaseName || x.Email == lowercaseEmail)
                    .Limit(2)
                    .ToList();

                foreach (var x in has)
                {
                    badName |= string.Equals(x.Name, lowercaseName, StringComparison.OrdinalIgnoreCase);
                    badEmail |= string.Equals(x.Email, lowercaseEmail, StringComparison.OrdinalIgnoreCase);
                }

                if (badName || badEmail)
                    return null;

#warning TODO: missing transaction supporting in litedb 4.0.0-beta1
                var user = new User
                {
                    Name = name,
                    Nickname = nickname,
                    Email = email,
                    Description = description,
                    CreationDate = DateTime.Now,
                };

                db.Insert(user);

                var pp = PasswordProvider.Peek();
                user.PasswordVersion = pp.Version;
                user.Password = pp.Compute(user.ID, name, password);

                db.Update(user);

                return user;
            }
        }

        public User Login(string id, string password)
        {
            var lowercaseId = id.ToLower();

            using (var db = _dbAccessor.CreateMainDbAccessor())
            {
                var user = db
                    .FirstOrDefault<User>(x => x.Name == lowercaseId || x.Email == lowercaseId);
                if (user != null)
                {
                    var pp = PasswordProvider.Peek(user.PasswordVersion);
                    if (user.Password == pp.Compute(user.ID, user.Name, password))
                    {
                        if (user.PasswordVersion != PasswordProvider.LastVersion)
                        {
                            pp = PasswordProvider.Peek();
                            user.Password = pp.Compute(user.ID, user.Name, password);
                            user.PasswordVersion = pp.Version;

                            db.Update(user);
                        }
                        return user;
                    }
                }
                return null;
            }
        }
    }
}
