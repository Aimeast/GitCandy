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

        public User CreateUser(string name, string nickname, string password, string email, string description, out bool badName, out bool badEmail)
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

        public AuthorizationLog CreateAuthorization(long userID, DateTime expires, string ip)
        {
            using (var db = _dbAccessor.CreateMainDbAccessor())
            {
                var auth = new AuthorizationLog
                {
                    AuthCode = Guid.NewGuid(),
                    UserID = userID,
                    IssueDate = DateTime.Now,
                    Expires = expires,
                    IssueIp = ip,
                    LastIp = ip,
                    IsValid = true,
                };
                db.Insert(auth);
                return auth;
            }
        }

        public void UpdateAuthorization(Guid authCode, DateTime expires, string lastIp)
        {
            using (var db = _dbAccessor.CreateMainDbAccessor())
            {
                var auth = db.FirstOrDefault<AuthorizationLog>(x => x.AuthCode == authCode);
                if (auth != null)
                {
                    auth.Expires = expires;
                    auth.LastIp = lastIp;

                    db.Update(auth);
                }
            }
        }

        public void SetAuthorizationAsInvalid(Guid authCode)
        {
            using (var db = _dbAccessor.CreateMainDbAccessor())
            {
                var auth = db.FirstOrDefault<AuthorizationLog>(x => x.AuthCode == authCode);
                if (auth != null)
                {
                    auth.IsValid = false;

                    db.Update(auth);
                }
            }
        }

        public Token GetAuthorizationToken(Guid authCode)
        {
            using (var db = _dbAccessor.CreateMainDbAccessor())
            {
                var auth = db.FirstOrDefault<AuthorizationLog>(x => x.AuthCode == authCode && x.IsValid);
                if (auth == null)
                {
                    return null;
                }
                var user = db.First<User>(x => x.ID == auth.UserID);
                return new Token(auth.AuthCode, user.ID, user.Name, user.Nickname, user.IsSystemAdministrator, auth.Expires)
                {
                    LastIp = auth.LastIp,
                };
            }
        }
    }
}
