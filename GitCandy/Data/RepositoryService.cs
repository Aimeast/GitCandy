using GitCandy.DAL;
using GitCandy.Models;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;

namespace GitCandy.Data
{
    [Export(typeof(RepositoryService))]
    public class RepositoryService
    {
        public Repository Create(RepositoryModel model, long managerID, out bool badName)
        {
            using (var ctx = new GitCandyContext())
            //using (TransactionScope transaction = new TransactionScope())
            {
                badName = ctx.Repositories.Any(s => s.Name == model.Name);
                if (badName)
                    return null;

                var repo = new Repository
                {
                    Name = model.Name,
                    Description = model.Description,
                    CreationDate = DateTime.UtcNow,
                    IsPrivate = model.IsPrivate,
                    AllowAnonymousRead = model.AllowAnonymousRead,
                    AllowAnonymousWrite = model.AllowAnonymousWrite,
                };
                ctx.Repositories.Add(repo);
                ctx.SaveChanges();

                if (managerID > 0)
                {
                    repo.UserRepositoryRoles.Add(new UserRepositoryRole
                    {
                        Repository = repo,
                        UserID = managerID,
                        IsOwner = true,
                        AllowRead = true,
                        AllowWrite = true
                    });
                }
                ctx.SaveChanges();

                //transaction.Complete();
                return repo;
            }
        }

        public RepositoryModel GetRepositoryModel(string name, bool withShipment = false)
        {
            using (var ctx = new GitCandyContext())
            {
                var repo = ctx.Repositories.FirstOrDefault(s => s.Name == name);
                if (repo == null)
                    return null;

                var model = new RepositoryModel
                {
                    Name = repo.Name,
                    Description = repo.Description,
                    IsPrivate = repo.IsPrivate,
                    AllowAnonymousRead = repo.AllowAnonymousRead,
                    AllowAnonymousWrite = repo.AllowAnonymousWrite,
                };
                if (withShipment)
                {
                    model.Collaborators = repo.UserRepositoryRoles.Select(s => s.User.Name).ToArray();
                    model.Teams = repo.TeamRepositoryRoles.Select(s => s.Team.Name).ToArray();
                }
                return model;
            }
        }

        public bool Update(RepositoryModel model)
        {
            using (var ctx = new GitCandyContext())
            {
                var repo = ctx.Repositories.FirstOrDefault(s => s.Name == model.Name);
                if (repo != null)
                {
                    repo.IsPrivate = model.IsPrivate;
                    repo.AllowAnonymousRead = model.AllowAnonymousRead;
                    repo.AllowAnonymousWrite = model.AllowAnonymousWrite;
                    repo.Description = model.Description;

                    ctx.SaveChanges();
                    return true;
                }
                return false;
            }
        }

        public CollaborationModel GetRepositoryCollaborationModel(string name)
        {
            using (var ctx = new GitCandyContext())
            {
                var repo = ctx.Repositories.FirstOrDefault(s => s.Name == name);
                if (repo == null)
                    return null;

                var model = new CollaborationModel
                {
                    RepositoryName = repo.Name,
                    Users = repo.UserRepositoryRoles
                        .Select(s => new CollaborationModel.UserRole
                        {
                            Name = s.User.Name,
                            AllowRead = s.AllowRead,
                            AllowWrite = s.AllowWrite,
                            IsOwner = s.IsOwner,
                        })
                        .OrderBy(s => s.Name)
                        .ToArray(),
                    Teams = repo.TeamRepositoryRoles
                        .Select(s => new CollaborationModel.TeamRole
                        {
                            Name = s.Team.Name,
                            AllowRead = s.AllowRead,
                            AllowWrite = s.AllowWrite,
                        })
                        .OrderBy(s => s.Name)
                        .ToArray(),
                };
                return model;
            }
        }

        public UserRepositoryRole RepositoryAddUser(string reponame, string username)
        {
            using (var ctx = new GitCandyContext())
            {
                var user = ctx.Users.FirstOrDefault(s => s.Name == username);
                if (user == null)
                    return null;
                var repo = ctx.Repositories.FirstOrDefault(s => s.Name == reponame);
                if (repo == null)
                    return null;
                if (repo.UserRepositoryRoles.Any(s => s.User.Name == username))
                    return null;

                var role = new UserRepositoryRole
                {
                    Repository = repo,
                    User = user,
                    AllowRead = true,
                    AllowWrite = true,
                    IsOwner = false,
                };
                repo.UserRepositoryRoles.Add(role);

                ctx.SaveChanges();
                return role;
            }
        }

        public bool RepositoryRemoveUser(string reponame, string username)
        {
            using (var ctx = new GitCandyContext())
            {
                var repo = ctx.Repositories.FirstOrDefault(s => s.Name == reponame);
                if (repo == null)
                    return false;
                var role = repo.UserRepositoryRoles.FirstOrDefault(s => s.User.Name == username);
                if (role == null)
                    return false;

                repo.UserRepositoryRoles.Remove(role);

                ctx.SaveChanges();
                return true;
            }
        }

        public bool RepositoryUserSetValue(string reponame, string username, string field, bool value)
        {
            using (var ctx = new GitCandyContext())
            {
                var repo = ctx.Repositories.FirstOrDefault(s => s.Name == reponame);
                if (repo == null)
                    return false;
                var role = repo.UserRepositoryRoles.FirstOrDefault(s => s.User.Name == username);
                if (role == null)
                    return false;

                if (field == "read")
                    role.AllowRead = value;
                else if (field == "write")
                    role.AllowWrite = value;
                else if (field == "owner")
                    role.IsOwner = value;
                else
                    return false;

                ctx.SaveChanges();
                return true;
            }
        }

        public TeamRepositoryRole RepositoryAddTeam(string reponame, string teamname)
        {
            using (var ctx = new GitCandyContext())
            {
                var team = ctx.Teams.FirstOrDefault(s => s.Name == teamname);
                if (team == null)
                    return null;
                var repo = ctx.Repositories.FirstOrDefault(s => s.Name == reponame);
                if (repo == null)
                    return null;
                if (repo.TeamRepositoryRoles.Any(s => s.Team.Name == teamname))
                    return null;

                var role = new TeamRepositoryRole
                {
                    Repository = repo,
                    Team = team,
                    AllowRead = true,
                    AllowWrite = true,
                };
                repo.TeamRepositoryRoles.Add(role);

                ctx.SaveChanges();
                return role;
            }
        }

        public bool RepositoryRemoveTeam(string reponame, string teamname)
        {
            using (var ctx = new GitCandyContext())
            {
                var repo = ctx.Repositories.FirstOrDefault(s => s.Name == reponame);
                if (repo == null)
                    return false;
                var role = repo.TeamRepositoryRoles.FirstOrDefault(s => s.Team.Name == teamname);
                if (role == null)
                    return false;

                repo.TeamRepositoryRoles.Remove(role);

                ctx.SaveChanges();
                return true;
            }
        }

        public bool RepositoryTeamSetValue(string reponame, string teamname, string field, bool value)
        {
            using (var ctx = new GitCandyContext())
            {
                var repo = ctx.Repositories.FirstOrDefault(s => s.Name == reponame);
                if (repo == null)
                    return false;
                var role = repo.TeamRepositoryRoles.FirstOrDefault(s => s.Team.Name == teamname);
                if (role == null)
                    return false;

                if (field == "read")
                    role.AllowRead = value;
                else if (field == "write")
                    role.AllowWrite = value;
                else
                    return false;

                ctx.SaveChanges();
                return true;
            }
        }

        public void Delete(string name)
        {
            using (var ctx = new GitCandyContext())
            {
                var repo = ctx.Repositories.FirstOrDefault(s => s.Name == name);
                if (repo != null)
                {
                    repo.TeamRepositoryRoles.Clear();
                    repo.UserRepositoryRoles.Clear();
                    ctx.Repositories.Remove(repo);
                    ctx.SaveChanges();
                }
            }
        }

        public bool IsRepositoryAdministrator(string reponame, string username)
        {
            using (var ctx = new GitCandyContext())
            {
                var repo = ctx.Repositories.FirstOrDefault(s => s.Name == reponame);
                if (repo == null)
                    return false;

                var role = repo.UserRepositoryRoles.FirstOrDefault(s => s.User.Name == username);
                return role != null && role.IsOwner;
            }
        }

        public bool CanReadRepository(string reponame, string username)
        {
            using (var ctx = new GitCandyContext())
            {
                var repo = ctx.Repositories.FirstOrDefault(s => s.Name == reponame);
                if (repo == null)
                    return false;

                if (repo.AllowAnonymousRead)
                    return true;

                if (!string.IsNullOrEmpty(username))
                {
                    if (repo.UserRepositoryRoles.Any(s => s.User.Name == username && s.AllowRead))
                        return true;

                    if (repo.TeamRepositoryRoles.Any(s => s.Team.UserTeamRoles.Any(t => t.User.Name == username) && s.AllowRead))
                        return true;
                }

                return false;
            }
        }

        public bool CanWriteRepository(string reponame, string username)
        {
            using (var ctx = new GitCandyContext())
            {
                var repo = ctx.Repositories.FirstOrDefault(s => s.Name == reponame);
                if (repo == null)
                    return false;

                if (repo.AllowAnonymousRead && repo.AllowAnonymousWrite)
                    return true;

                if (!string.IsNullOrEmpty(username))
                {
                    if (repo.UserRepositoryRoles.Any(s => s.User.Name == username && s.AllowRead && s.AllowWrite))
                        return true;

                    if (repo.TeamRepositoryRoles.Any(s => s.Team.UserTeamRoles.Any(t => t.User.Name == username) && s.AllowRead && s.AllowWrite))
                        return true;
                }

                return false;
            }
        }

        public RepositoryListModel GetRepositories(string username, bool showAll = false)
        {
            using (var ctx = new GitCandyContext())
            {
                var model = new RepositoryListModel();

                if (string.IsNullOrEmpty(username))
                {
                    model.Collaborations = new RepositoryModel[0];
                    model.Repositories = ToRepositoryArray(ctx.Repositories.Where(s => !s.IsPrivate).OrderBy(s => s.Name));
                }
                else
                {
                    var q1 = ctx.UserRepositoryRoles.Where(s => s.User.Name == username && s.AllowRead && s.AllowWrite).Select(s => s.Repository);
                    var q2 = ctx.UserTeamRoles.Where(s => s.User.Name == username).SelectMany(s => s.Team.TeamRepositoryRoles.Where(t => t.AllowRead && t.AllowWrite).Select(t => t.Repository));
                    var q3 = q1.Union(q2);

                    model.Collaborations = ToRepositoryArray(q3.OrderBy(s => s.Name));
                    model.Repositories = ToRepositoryArray(ctx.Repositories.Where(s => showAll || (!s.IsPrivate)).Except(q3).OrderBy(s => s.Name));
                }

                return model;
            }
        }

        private RepositoryModel[] ToRepositoryArray(IEnumerable<Repository> source)
        {
            return source.Select(s => new RepositoryModel
                    {
                        Name = s.Name,
                        Description = s.Description,
                    })
                    .ToArray();
        }
    }
}