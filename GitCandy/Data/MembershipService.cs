using GitCandy.DAL;
using GitCandy.Models;
using GitCandy.Security;
using System;
using System.Composition;
using System.Linq;

namespace GitCandy.Data
{
    [Export(typeof(MembershipService))]
    public class MembershipService
    {
        #region Account part
        public User CreateAccount(string name, string nickname, string password, string email, string description, out bool badName, out bool badEmail)
        {
            badName = false;
            badEmail = false;

            using (var ctx = new GitCandyContext())
            //using (TransactionScope transaction = new TransactionScope()) // I don't know why Sqlite not support for TransactionScope
            {
                try
                {
                    var list = ctx.Users.Where(s => s.Name == name || s.Email == email).ToList();
                    badName = list.Any(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
                    badEmail = list.Any(s => string.Equals(s.Email, email, StringComparison.OrdinalIgnoreCase));

                    if (badName || badEmail)
                        return null;

                    var user = new User
                    {
                        Name = name,
                        Nickname = nickname,
                        Email = email,
                        PasswordVersion = -1,
                        Password = "",
                        Description = description,
                        CreationDate = DateTime.UtcNow,
                    };
                    ctx.Users.Add(user);
                    ctx.SaveChanges();

                    using (var pp = PasswordProviderPool.Take())
                    {
                        user.PasswordVersion = pp.Version;
                        user.Password = pp.Compute(user.ID, name, password);
                    }
                    ctx.SaveChanges();

                    //transaction.Complete();
                    return user;
                }
                catch
                {
                    return null;
                }
            }
        }

        public UserModel GetUserModel(string name, bool withMembers = false, string viewUser = null)
        {
            using (var ctx = new GitCandyContext())
            {
                var user = ctx.Users.FirstOrDefault(s => s.Name == name);

                if (user == null)
                    return null;

                var model = new UserModel
                {
                    Name = user.Name,
                    Nickname = user.Nickname,
                    Email = user.Email,
                    Description = user.Description,
                    IsSystemAdministrator = user.IsSystemAdministrator,
                };
                if (withMembers)
                {
                    model.Teams = ctx.UserTeamRoles
                        .Where(s => s.User.ID == user.ID)
                        .OrderBy(s => s.Team.Name)
                        .Select(s => s.Team.Name)
                        .ToArray();

                    model.Respositories = ctx.UserRepositoryRoles
                        // belong user
                        .Where(s => s.User.ID == user.ID && s.IsOwner)
                        // can view for viewUser
                        .Where(s => !s.Repository.IsPrivate
                            || viewUser != null &&
                                (ctx.Users.Any(t => t.Name == viewUser && t.IsSystemAdministrator)
                                || ctx.UserRepositoryRoles.Any(t => t.RepositoryID == s.RepositoryID
                                    && t.User.Name == viewUser
                                    && t.AllowRead)
                                || ctx.TeamRepositoryRoles.Any(t => t.RepositoryID == s.RepositoryID
                                    && t.Team.UserTeamRoles.Any(r => r.User.Name == viewUser)
                                    && t.AllowRead)))
                        .OrderBy(s => s.Repository.Name)
                        .Select(s => s.Repository.Name)
                        .ToArray();
                }
                return model;
            }
        }

        public User Login(string id, string password)
        {
            using (var ctx = new GitCandyContext())
            {
                var user = ctx.Users.FirstOrDefault(s => s.Name == id || s.Email == id);
                if (user != null)
                {
                    using (var pp1 = PasswordProviderPool.Take(user.PasswordVersion))
                        if (user.Password == pp1.Compute(user.ID, user.Name, password))
                        {
                            if (user.PasswordVersion != PasswordProviderPool.LastVersion)
                                using (var pp2 = PasswordProviderPool.Take())
                                {
                                    user.Password = pp2.Compute(user.ID, user.Name, password);
                                    user.PasswordVersion = pp2.Version;
                                    ctx.SaveChanges();
                                }
                            return user;
                        }
                }
                return null;
            }
        }

        public void SetPassword(string name, string newPassword)
        {
            using (var ctx = new GitCandyContext())
            {
                var user = ctx.Users.FirstOrDefault(s => s.Name == name);
                if (user != null)
                {
                    using (var pp = PasswordProviderPool.Take())
                    {
                        user.Password = pp.Compute(user.ID, user.Name, newPassword);
                        user.PasswordVersion = pp.Version;
                    }

                    var auths = ctx.AuthorizationLogs.Where(s => s.UserID == user.ID);
                    foreach (var auth in auths)
                    {
                        auth.IsValid = false;
                    }
                    ctx.SaveChanges();
                }
            }
        }

        public bool UpdateUser(UserModel model)
        {
            using (var ctx = new GitCandyContext())
            {
                var user = ctx.Users.FirstOrDefault(s => s.Name == model.Name);
                if (user != null)
                {
                    user.Nickname = model.Nickname;
                    user.Email = model.Email;
                    user.Description = model.Description;
                    user.IsSystemAdministrator = model.IsSystemAdministrator;

                    ctx.SaveChanges();
                    return true;
                }
                return false;
            }
        }

        public AuthorizationLog CreateAuthorization(long userID, DateTime expires, string ip)
        {
            using (var ctx = new GitCandyContext())
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
                ctx.AuthorizationLogs.Add(auth);
                ctx.SaveChanges();
                return auth;
            }
        }

        public Token GetToken(Guid authCode)
        {
            using (var ctx = new GitCandyContext())
            {
                var meta = ctx.AuthorizationLogs
                    .Where(s => s.AuthCode == authCode && s.IsValid)
                    .Select(s => new
                    {
                        s.AuthCode,
                        s.Expires,
                        s.User.ID,
                        s.User.Name,
                        s.User.Nickname,
                        s.User.IsSystemAdministrator,
                        s.LastIp,
                    })
                    .FirstOrDefault();
                return meta == null
                    ? null
                    : new Token(meta.AuthCode, meta.ID, meta.Name, meta.Nickname, meta.IsSystemAdministrator, meta.Expires)
                    {
                        LastIp = meta.LastIp
                    };
            }
        }

        public void UpdateAuthorization(Guid authCode, DateTime expires, string lastIp)
        {
            using (var ctx = new GitCandyContext())
            {
                var auth = ctx.AuthorizationLogs.FirstOrDefault(s => s.AuthCode == authCode);
                if (auth != null)
                {
                    auth.Expires = expires;
                    auth.LastIp = lastIp;
                    ctx.SaveChanges();
                }
            }
        }

        public void SetAuthorizationAsInvalid(Guid authCode)
        {
            using (var ctx = new GitCandyContext())
            {
                var auth = ctx.AuthorizationLogs.FirstOrDefault(s => s.AuthCode == authCode);
                if (auth != null)
                {
                    auth.IsValid = false;
                    ctx.SaveChanges();
                }
            }
        }

        public void DeleteUser(string name)
        {
            using (var ctx = new GitCandyContext())
            {
                var user = ctx.Users.FirstOrDefault(s => s.Name == name);
                if (user != null)
                {
                    user.UserTeamRoles.Clear();
                    user.UserRepositoryRoles.Clear();
                    user.AuthorizationLogs.Clear();
                    ctx.Users.Remove(user);
                    ctx.SaveChanges();
                }
            }
        }

        public UserListModel GetUserList(string keyword, int page, int pagesize = 20)
        {
            using (var ctx = new GitCandyContext())
            {
                var query = ctx.Users.AsQueryable();
                if (!string.IsNullOrEmpty(keyword))
                    query = query.Where(s => s.Name.Contains(keyword)
                        || s.Nickname.Contains(keyword)
                        || s.Email.Contains(keyword)
                        || s.Description.Contains(keyword));
                query = query.OrderBy(s => s.Name);

                var model = new UserListModel
                {
                    Users = query
                        .Skip((page - 1) * pagesize)
                        .Take(pagesize)
                        .Select(user => new UserModel
                        {
                            Name = user.Name,
                            Nickname = user.Nickname,
                            Email = user.Email,
                            Description = user.Description,
                            IsSystemAdministrator = user.IsSystemAdministrator,
                        })
                        .ToArray(),
                    CurrentPage = page,
                    ItemCount = query.Count(),
                };
                return model;
            }
        }

        public string[] SearchUsers(string query)
        {
            using (var ctx = new GitCandyContext())
            {
                var length = query.Length + 0.5;
                return ctx.Users
                    .Where(s => s.Name.Contains(query))
                    .OrderByDescending(s => length / s.Name.Length)
                    .ThenBy(s => s.Name)
                    .Take(10)
                    .Select(s => s.Name)
                    .ToArray();
            }
        }
        #endregion

        #region Team part
        public Team CreateTeam(string name, string description, long managerID, out bool badName)
        {
            badName = false;

            using (var ctx = new GitCandyContext())
            //using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    badName = ctx.Teams.Count(s => s.Name == name) != 0;

                    if (badName)
                        return null;

                    var team = new Team
                    {
                        Name = name,
                        Description = description,
                        CreationDate = DateTime.UtcNow,
                    };
                    ctx.Teams.Add(team);

                    if (managerID > 0)
                    {
                        team.UserTeamRoles.Add(new UserTeamRole { Team = team, UserID = managerID, IsAdministrator = true });
                    }
                    ctx.SaveChanges();

                    //transaction.Complete();
                    return team;
                }
                catch
                {
                    return null;
                }
            }
        }

        public bool UpdateTeam(TeamModel model)
        {
            using (var ctx = new GitCandyContext())
            {
                var team = ctx.Teams.FirstOrDefault(s => s.Name == model.Name);
                if (team != null)
                {
                    team.Description = model.Description;
                    ctx.SaveChanges();
                    return true;
                }
                return false;
            }
        }

        public TeamModel GetTeamModel(string name, bool withMembers = false, string viewUser = null)
        {
            using (var ctx = new GitCandyContext())
            {
                var team = ctx.Teams.FirstOrDefault(s => s.Name == name);
                if (team == null)
                    return null;

                var model = new TeamModel
                {
                    Name = team.Name,
                    Description = team.Description,
                };
                if (withMembers)
                {
                    model.MembersRole = ctx.UserTeamRoles
                        .Where(s => s.TeamID == team.ID)
                        .OrderBy(s => s.User.Name)
                        .Select(s => new TeamModel.UserRole
                        {
                            Name = s.User.Name,
                            IsAdministrator = s.IsAdministrator
                        })
                        .ToArray();
                    model.Members = model.MembersRole.Select(s => s.Name).ToArray();

                    model.RepositoriesRole = ctx.TeamRepositoryRoles
                        // belong team
                        .Where(s => s.TeamID == team.ID)
                        // can view for viewUser
                        .Where(s => !s.Repository.IsPrivate
                            || viewUser != null &&
                                (ctx.Users.Any(t => t.Name == viewUser && t.IsSystemAdministrator)
                                || ctx.UserRepositoryRoles.Any(t => t.RepositoryID == s.RepositoryID
                                    && t.User.Name == viewUser
                                    && t.AllowRead)
                                || ctx.TeamRepositoryRoles.Any(t => t.RepositoryID == s.RepositoryID
                                    && t.Team.UserTeamRoles.Any(r => r.User.Name == viewUser)
                                    && t.AllowRead)))
                        .OrderBy(s => s.Repository.Name)
                        .Select(s => new TeamModel.RepositoryRole
                        {
                            Name = s.Repository.Name,
                            AllowRead = s.AllowRead,
                            AllowWrite = s.AllowWrite,
                        })
                        .ToArray();
                    model.Repositories = model.RepositoriesRole.Select(s => s.Name).ToArray();
                }
                return model;
            }
        }

        public bool TeamAddUser(string teamname, string username)
        {
            using (var ctx = new GitCandyContext())
            {
                var pair = (from t in ctx.Teams
                            from u in ctx.Users
                            where t.Name == teamname && u.Name == username
                                && t.UserTeamRoles.All(r => r.UserID != u.ID)
                            select new { TeamID = t.ID, UserID = u.ID })
                            .FirstOrDefault();
                if (pair == null)
                    return false;

                ctx.UserTeamRoles.Add(new UserTeamRole { TeamID = pair.TeamID, UserID = pair.UserID, IsAdministrator = false });

                ctx.SaveChanges();
                return true;
            }
        }

        public bool TeamRemoveUser(string teamname, string username)
        {
            using (var ctx = new GitCandyContext())
            {
                var role = ctx.UserTeamRoles.FirstOrDefault(s => s.Team.Name == teamname && s.User.Name == username);
                if (role == null)
                    return false;

                ctx.UserTeamRoles.Remove(role);

                ctx.SaveChanges();
                return true;
            }
        }

        public bool TeamUserSetAdministrator(string teamname, string username, bool isAdmin)
        {
            using (var ctx = new GitCandyContext())
            {
                var role = ctx.UserTeamRoles.FirstOrDefault(s => s.Team.Name == teamname && s.User.Name == username);
                if (role == null)
                    return false;

                role.IsAdministrator = isAdmin;

                ctx.SaveChanges();
                return true;
            }
        }

        public string[] SearchTeam(string query)
        {
            using (var ctx = new GitCandyContext())
            {
                var length = query.Length + 0.5;
                return ctx.Teams
                    .Where(s => s.Name.Contains(query))
                    .OrderByDescending(s => length / s.Name.Length)
                    .ThenBy(s => s.Name)
                    .Take(10)
                    .Select(s => s.Name)
                    .ToArray();
            }
        }

        public bool IsTeamAdministrator(string teamname, string username)
        {
            using (var ctx = new GitCandyContext())
            {
                var role = ctx.UserTeamRoles.FirstOrDefault(s => s.Team.Name == teamname && s.User.Name == username);
                return role != null && role.IsAdministrator;
            }
        }

        public void DeleteTeam(string name)
        {
            using (var ctx = new GitCandyContext())
            {
                var team = ctx.Teams.FirstOrDefault(s => s.Name == name);
                if (team != null)
                {
                    team.UserTeamRoles.Clear();
                    team.TeamRepositoryRoles.Clear();
                    ctx.Teams.Remove(team);
                    ctx.SaveChanges();
                }
            }
        }

        public TeamListModel GetTeamList(string keyword, int page, int pagesize = 20)
        {
            using (var ctx = new GitCandyContext())
            {
                var query = ctx.Teams.AsQueryable();
                if (!string.IsNullOrEmpty(keyword))
                    query = query.Where(s => s.Name.Contains(keyword)
                        || s.Description.Contains(keyword));
                query = query.OrderBy(s => s.Name);

                var model = new TeamListModel
                {
                    Teams = query
                        .Skip((page - 1) * pagesize)
                        .Take(pagesize)
                        .Select(s => new TeamModel
                        {
                            Name = s.Name,
                            Description = s.Description,
                        })
                        .ToArray(),
                    CurrentPage = page,
                    ItemCount = query.Count(),
                };
                return model;
            }
        }
        #endregion
    }
}