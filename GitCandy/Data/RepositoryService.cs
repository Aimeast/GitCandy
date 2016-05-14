using GitCandy.Base;
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

        public RepositoryModel GetRepositoryModel(string reponame, bool withShipment = false, string username = null)
        {
            using (var ctx = new GitCandyContext())
            {
                var repo = ctx.Repositories.FirstOrDefault(s => s.Name == reponame);
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
                if (withShipment || username != null)
                {
                    var tempList = ctx.UserRepositoryRoles
                        .Where(s => s.Repository.Name == reponame)
                        .Select(s => new { s.User.Name, s.IsOwner, Kind = true })
                        .Concat(ctx.TeamRepositoryRoles
                            .Where(s => s.Repository.Name == reponame)
                            .Select(s => new { s.Team.Name, IsOwner = false, Kind = false }))
                        .ToList();

                    if (withShipment)
                    {
                        model.Collaborators = tempList
                            .Where(s => s.Kind)
                            .Select(s => s.Name)
                            .OrderBy(s => s, new StringLogicalComparer())
                            .ToArray();
                        model.Teams = tempList
                            .Where(s => !s.Kind)
                            .Select(s => s.Name)
                            .OrderBy(s => s, new StringLogicalComparer())
                            .ToArray();
                    }
                    if (username != null)
                    {
                        model.CurrentUserIsOwner = tempList
                            .Any(s => s.Kind && s.IsOwner && s.Name == username);
                    }
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
                        .OrderBy(s => s.Name, new StringLogicalComparer())
                        .ToArray(),
                    Teams = repo.TeamRepositoryRoles
                        .Select(s => new CollaborationModel.TeamRole
                        {
                            Name = s.Team.Name,
                            AllowRead = s.AllowRead,
                            AllowWrite = s.AllowWrite,
                        })
                        .OrderBy(s => s.Name, new StringLogicalComparer())
                        .ToArray(),
                };
                return model;
            }
        }

        public UserRepositoryRole RepositoryAddUser(string reponame, string username)
        {
            using (var ctx = new GitCandyContext())
            {
                var pair = (from r in ctx.Repositories
                            from u in ctx.Users
                            where r.Name == reponame && u.Name == username
                                && r.UserRepositoryRoles.All(s => s.User.Name != username)
                            select new { RepoID = r.ID, UserID = u.ID })
                            .FirstOrDefault();
                if (pair == null)
                    return null;

                var role = new UserRepositoryRole
                {
                    RepositoryID = pair.RepoID,
                    UserID = pair.UserID,
                    AllowRead = true,
                    AllowWrite = true,
                    IsOwner = false,
                };
                ctx.UserRepositoryRoles.Add(role);
                ctx.SaveChanges();
                return role;
            }
        }

        public bool RepositoryRemoveUser(string reponame, string username)
        {
            using (var ctx = new GitCandyContext())
            {
                var role = ctx.UserRepositoryRoles.FirstOrDefault(s => s.Repository.Name == reponame && s.User.Name == username);
                if (role == null)
                    return false;

                ctx.UserRepositoryRoles.Remove(role);
                ctx.SaveChanges();
                return true;
            }
        }

        public bool RepositoryUserSetValue(string reponame, string username, string field, bool value)
        {
            using (var ctx = new GitCandyContext())
            {
                var role = ctx.UserRepositoryRoles.FirstOrDefault(s => s.Repository.Name == reponame && s.User.Name == username);
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
                var pair = (from r in ctx.Repositories
                            from t in ctx.Teams
                            where r.Name == reponame && t.Name == teamname
                                && r.TeamRepositoryRoles.All(s => s.Team.Name != teamname)
                            select new { RepoID = r.ID, TeamID = t.ID })
                            .FirstOrDefault();
                if (pair == null)
                    return null;

                var role = new TeamRepositoryRole
                {
                    RepositoryID = pair.RepoID,
                    TeamID = pair.TeamID,
                    AllowRead = true,
                    AllowWrite = true,
                };
                ctx.TeamRepositoryRoles.Add(role);
                ctx.SaveChanges();
                return role;
            }
        }

        public bool RepositoryRemoveTeam(string reponame, string teamname)
        {
            using (var ctx = new GitCandyContext())
            {
                var role = ctx.TeamRepositoryRoles.FirstOrDefault(s => s.Repository.Name == reponame && s.Team.Name == teamname);
                if (role == null)
                    return false;

                ctx.TeamRepositoryRoles.Remove(role);
                ctx.SaveChanges();
                return true;
            }
        }

        public bool RepositoryTeamSetValue(string reponame, string teamname, string field, bool value)
        {
            using (var ctx = new GitCandyContext())
            {
                var role = ctx.TeamRepositoryRoles.FirstOrDefault(s => s.Repository.Name == reponame && s.Team.Name == teamname);
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
                var role = ctx.UserRepositoryRoles.FirstOrDefault(s => s.Repository.Name == reponame && s.User.Name == username);
                return role != null && role.IsOwner;
            }
        }

        public bool CanReadRepository(string reponame, string username)
        {
            using (var ctx = new GitCandyContext())
            {
                var q0 = ctx.Repositories.Where(s => s.Name == reponame && s.AllowAnonymousRead).Select(s => 0);
                if (string.IsNullOrEmpty(username))
                    return q0.Any();

                var q1 = ctx.UserRepositoryRoles.Where(s => s.Repository.Name == reponame && s.User.Name == username && s.AllowRead).Select(s => 0);
                var q2 = ctx.TeamRepositoryRoles.Where(s => s.Repository.Name == reponame && s.Team.UserTeamRoles.Any(t => t.User.Name == username) && s.AllowRead).Select(s => 0);
                var q3 = ctx.Repositories.Where(s => s.Name == reponame && ctx.Users.Any(u => u.Name == username && u.IsSystemAdministrator)).Select(s => 0);
                return q0.Concat(q1).Concat(q2).Concat(q3).Any();
            }
        }

        public bool CanWriteRepository(string reponame, string username)
        {
            using (var ctx = new GitCandyContext())
            {
                var q0 = ctx.Repositories.Where(s => s.Name == reponame && s.AllowAnonymousRead && s.AllowAnonymousWrite).Select(s => 0);
                if (string.IsNullOrEmpty(username))
                    return q0.Any();

                var q1 = ctx.UserRepositoryRoles.Where(s => s.Repository.Name == reponame && s.User.Name == username && s.AllowRead && s.AllowWrite).Select(s => 0);
                var q2 = ctx.TeamRepositoryRoles.Where(s => s.Repository.Name == reponame && s.Team.UserTeamRoles.Any(t => t.User.Name == username) && s.AllowRead && s.AllowWrite).Select(s => 0);
                var q3 = ctx.Repositories.Where(s => s.Name == reponame && ctx.Users.Any(u => u.Name == username && u.IsSystemAdministrator)).Select(s => 0);
                return q0.Concat(q1).Concat(q2).Concat(q3).Any();
            }
        }

        public bool CanReadRepository(string reponame, string fingerprint, string publickey)
        {
            using (var ctx = new GitCandyContext())
            {
                var q0 = ctx.Repositories.Where(s => s.Name == reponame && s.AllowAnonymousRead).Select(s => 0);
                var q1 = ctx.UserRepositoryRoles
                    .Where(s => s.Repository.Name == reponame
                        && s.User.SshKeys.Any(t => t.Fingerprint == fingerprint && t.PublicKey == publickey)
                        && s.AllowRead)
                    .Select(s => 0);
                var q2 = ctx.TeamRepositoryRoles
                    .Where(s => s.Repository.Name == reponame
                        && s.Team.UserTeamRoles.Any(t => t.User.SshKeys.Any(z => z.Fingerprint == fingerprint && z.PublicKey == publickey))
                        && s.AllowRead)
                    .Select(s => 0);
                return q0.Concat(q1).Concat(q2).Any();
            }
        }

        public bool CanWriteRepository(string reponame, string fingerprint, string publickey)
        {
            using (var ctx = new GitCandyContext())
            {
                var q0 = ctx.Repositories.Where(s => s.Name == reponame && s.AllowAnonymousRead && s.AllowAnonymousWrite).Select(s => 0);
                var q1 = ctx.UserRepositoryRoles
                    .Where(s => s.Repository.Name == reponame
                        && s.User.SshKeys.Any(t => t.Fingerprint == fingerprint && t.PublicKey == publickey)
                        && s.AllowRead && s.AllowWrite)
                    .Select(s => 0);
                var q2 = ctx.TeamRepositoryRoles
                    .Where(s => s.Repository.Name == reponame
                        && s.Team.UserTeamRoles.Any(t => t.User.SshKeys.Any(z => z.Fingerprint == fingerprint && z.PublicKey == publickey))
                        && s.AllowRead && s.AllowWrite)
                    .Select(s => 0);
                return q0.Concat(q1).Concat(q2).Any();
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