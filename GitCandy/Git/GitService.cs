using GitCandy.Base;
using GitCandy.Configuration;
using GitCandy.Extensions;
using GitCandy.Git.Cache;
using GitCandy.Models;
using GitCandy.Schedules;
using LibGit2Sharp;
using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitCandy.Git
{
    public class GitService : IDisposable
    {
        public const string UnknowString = "<Unknow>";

        private readonly Repository _repository;
        private readonly string _repositoryPath;
        private readonly string _repoId = null;
        private readonly Lazy<Encoding> _i18n;
        private bool _disposed;

        public Encoding I18n { get { return _i18n.Value; } }
        public string Name { get; private set; }

        public GitService(string name)
        {
            var info = GetDirectoryInfo(name);
            _repositoryPath = info.FullName;
            _repoId = Name = info.Name;

            if (!Repository.IsValid(_repositoryPath))
            {
                CreateRepository(name);
            }

            _repository = new Repository(_repositoryPath);
            _i18n = new Lazy<Encoding>(() =>
            {
                var entry = _repository.Config.Get<string>("i18n.commitEncoding");
                return entry == null
                    ? null
                    : CpToEncoding(entry.Value);
            });
        }

        #region Git Smart HTTP Transport
        public void InfoRefs(string service, Stream inStream, Stream outStream)
        {
            Contract.Requires(service == "receive-pack" || service == "upload-pack");
            RunGitCmd(service, true, inStream, outStream);
        }

        public void ExecutePack(string service, Stream inStream, Stream outStream)
        {
            Contract.Requires(service == "receive-pack" || service == "upload-pack");
            RunGitCmd(service, false, inStream, outStream);
        }
        #endregion

        #region Repository Browser
        public TreeModel GetTree(string path)
        {
            var isEmptyPath = string.IsNullOrEmpty(path);
            string referenceName;
            var commit = GetCommitByPath(ref path, out referenceName);
            if (commit == null)
            {
                if (isEmptyPath)
                {
                    var branch = _repository.Branches["master"]
                        ?? _repository.Branches.FirstOrDefault();
                    return new TreeModel
                    {
                        ReferenceName = branch == null ? "HEAD" : branch.FriendlyName,
                    };
                }
                return null;
            }

            var model = new TreeModel
            {
                ReferenceName = referenceName,
                Path = string.IsNullOrEmpty(path) ? "" : path,
                Commit = new CommitModel
                {
                    Sha = commit.Sha,
                    Author = commit.Author,
                    Committer = commit.Committer,
                    CommitMessageShort = commit.MessageShort.RepetitionIfEmpty(UnknowString),
                    Parents = commit.Parents.Select(s => s.Sha).ToArray()
                },
            };

            var tree = string.IsNullOrEmpty(path)
                ? commit.Tree
                : commit[path] == null
                    ? null
                    : commit[path].Target as Tree;
            if (tree == null)
                return null;

            var summaryAccessor = GitCacheAccessor.Singleton(new SummaryAccessor(_repoId, _repository, commit, tree));
            var items = summaryAccessor.Result.Value;
            var entries = (from entry in tree
                           join item in items on entry.Name equals item.Name into g
                           from item in g
                           select new TreeEntryModel
                           {
                               Name = entry.Name,
                               Path = entry.Path.Replace('\\', '/'),
                               Commit = new CommitModel
                               {
                                   Sha = item.CommitSha,
                                   CommitMessageShort = item.MessageShort,
                                   Author = CreateSafeSignature(item.AuthorName, item.AuthorEmail, item.AuthorWhen),
                                   Committer = CreateSafeSignature(item.CommitterName, item.CommitterEmail, item.CommitterWhen),
                               },
                               Sha = item.CommitSha,
                               EntryType = entry.TargetType,
                           })
                           .OrderBy(s => s.EntryType == TreeEntryTargetType.Blob)
                           .ThenBy(s => s.Name, new StringLogicalComparer())
                           .ToList();

            model.Entries = entries;
            model.Readme = entries.FirstOrDefault(s => s.EntryType == TreeEntryTargetType.Blob
                && (string.Equals(s.Name, "readme", StringComparison.OrdinalIgnoreCase)
                    //|| string.Equals(s.Name, "readme.txt", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(s.Name, "readme.md", StringComparison.OrdinalIgnoreCase)));

            if (model.Readme != null)
            {
                var data = ((Blob)tree[model.Readme.Name].Target).GetContentStream().ToBytes();
                var encoding = FileHelper.DetectEncoding(data, CpToEncoding(commit.Encoding), _i18n.Value);
                if (encoding == null)
                {
                    model.Readme.BlobType = BlobType.Binary;
                }
                else
                {
                    model.Readme.BlobType = model.Readme.Name.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
                        ? BlobType.MarkDown
                        : BlobType.Text;
                    model.Readme.TextContent = FileHelper.ReadToEnd(data, encoding);
                    model.Readme.TextBrush = "no-highlight";
                }
            }

            model.BranchSelector = GetBranchSelectorModel(referenceName, commit.Sha, path);
            model.PathBar = new PathBarModel
            {
                Name = Name,
                Action = "Tree",
                Path = path,
                ReferenceName = referenceName,
                ReferenceSha = commit.Sha,
                HideLastSlash = false,
            };

            if (model.IsRoot)
            {
                var scopeAccessor = GitCacheAccessor.Singleton(new ScopeAccessor(_repoId, _repository, commit));
                model.Scope = scopeAccessor.Result.Value;
            }

            return model;
        }

        public TreeEntryModel GetBlob(string path)
        {
            string referenceName;
            var commit = GetCommitByPath(ref path, out referenceName);
            if (commit == null)
                return null;

            var entry = commit[path];
            if (entry == null || entry.TargetType != TreeEntryTargetType.Blob)
                return null;

            var blob = (Blob)entry.Target;

            var cacheAccessor = GitCacheAccessor.Singleton(new LastCommitAccessor(_repoId, _repository, commit, path));
            var lastCommitSha = cacheAccessor.Result.Value;
            if (lastCommitSha != commit.Sha)
                commit = _repository.Lookup<Commit>(lastCommitSha);

            var data = blob.GetContentStream().ToBytes();
            var encoding = FileHelper.DetectEncoding(data, CpToEncoding(commit.Encoding), _i18n.Value);
            var extension = Path.GetExtension(entry.Name).ToLower();
            var model = new TreeEntryModel
            {
                Name = entry.Name,
                ReferenceName = referenceName,
                Sha = commit.Sha,
                Path = string.IsNullOrEmpty(path) ? "" : path,
                Commit = new CommitModel
                {
                    Sha = commit.Sha,
                    Author = commit.Author,
                    Committer = commit.Committer,
                    CommitMessage = commit.Message.RepetitionIfEmpty(UnknowString),
                    CommitMessageShort = commit.MessageShort.RepetitionIfEmpty(UnknowString),
                    Parents = commit.Parents.Select(s => s.Sha).ToArray()
                },
                EntryType = entry.TargetType,
                RawData = data,
                SizeString = FileHelper.GetSizeString(data.Length),
                TextContent = encoding == null
                    ? null
                    : FileHelper.ReadToEnd(data, encoding),
                TextBrush = FileHelper.BrushMapping.ContainsKey(extension)
                    ? FileHelper.BrushMapping[extension]
                    : "no-highlight",
                BlobType = encoding == null
                    ? FileHelper.ImageSet.Contains(extension)
                        ? BlobType.Image
                        : BlobType.Binary
                    : extension == ".md"
                        ? BlobType.MarkDown
                        : BlobType.Text,
                BlobEncoding = encoding,
                BranchSelector = GetBranchSelectorModel(referenceName, commit.Sha, path),
                PathBar = new PathBarModel
                {
                    Name = Name,
                    Action = "Tree",
                    Path = path,
                    ReferenceName = referenceName,
                    ReferenceSha = commit.Sha,
                    HideLastSlash = true,
                },
            };

            return model;
        }

        public CommitModel GetCommit(string path)
        {
            string referenceName;
            var commit = GetCommitByPath(ref path, out referenceName);
            if (commit == null)
                return null;

            var treeEntry = commit[path];
            var isBlob = treeEntry != null && treeEntry.TargetType == TreeEntryTargetType.Blob;
            var model = ToCommitModel(commit, referenceName, !isBlob, path);
            model.PathBar = new PathBarModel
            {
                Name = Name,
                Action = "Commit",
                Path = path,
                ReferenceName = referenceName,
                ReferenceSha = commit.Sha,
                HideLastSlash = isBlob,
            };
            return model;
        }

        public CompareModel GetCompare(string start, string end)
        {
            string name1, name2;
            var commit1 = GetCommitByPath(ref start, out name1);
            var commit2 = GetCommitByPath(ref end, out name2);
            if (commit1 == null)
            {
                commit1 = _repository.Head.Tip;
                name1 = _repository.Head.FriendlyName;
            }
            if (commit2 == null)
            {
                commit2 = _repository.Head.Tip;
                name2 = _repository.Head.FriendlyName;
            }

            var walks = _repository.Commits
                .QueryBy(new CommitFilter
                {
                    IncludeReachableFrom = commit2,
                    ExcludeReachableFrom = commit1,
                    SortBy = CommitSortStrategies.Time
                })
                .Select(s => new CommitModel
                {
                    Sha = s.Sha,
                    Committer = s.Committer,
                    CommitMessageShort = s.MessageShort.RepetitionIfEmpty(UnknowString),
                })
                .ToArray();

            var fromBranchSelector = GetBranchSelectorModel(name1, commit1.Sha, null);
            var toBranchSelector = GetBranchSelectorModel(name2, commit2.Sha, null);
            var model = new CompareModel
            {
                BaseBranchSelector = fromBranchSelector,
                CompareBranchSelector = toBranchSelector,
                CompareResult = ToCommitModel(commit1, name1, true, "", commit2.Tree),
                Walks = walks,
            };
            return model;
        }

        public CommitsModel GetCommits(string path, int page = 1, int pagesize = 20)
        {
            string referenceName;
            var commit = GetCommitByPath(ref path, out referenceName);
            if (commit == null)
                return null;

            var commitsAccessor = GitCacheAccessor.Singleton(new CommitsAccessor(_repoId, _repository, commit, path, page, pagesize));
            var scopeAccessor = GitCacheAccessor.Singleton(new ScopeAccessor(_repoId, _repository, commit, path));

            var model = new CommitsModel
            {
                ReferenceName = referenceName,
                Sha = commit.Sha,
                Commits = commitsAccessor.Result.Value
                    .Select(s => new CommitModel
                    {
                        CommitMessageShort = s.MessageShort,
                        Sha = s.CommitSha,
                        Author = CreateSafeSignature(s.AuthorName, s.AuthorEmail, s.AuthorWhen),
                        Committer = CreateSafeSignature(s.CommitterName, s.CommitterEmail, s.CommitterWhen),
                    })
                    .ToList(),
                CurrentPage = page,
                ItemCount = scopeAccessor.Result.Value.Commits,
                Path = string.IsNullOrEmpty(path) ? "" : path,
                PathBar = new PathBarModel
                {
                    Name = Name,
                    Action = "Commits",
                    Path = path,
                    ReferenceName = referenceName,
                    ReferenceSha = commit.Sha,
                    HideLastSlash = true, // I want a improvement here
                },
            };

            return model;
        }

        public BlameModel GetBlame(string path)
        {
            string referenceName;
            var commit = GetCommitByPath(ref path, out referenceName);
            if (commit == null)
                return null;

            var entry = commit[path];
            if (entry == null || entry.TargetType != TreeEntryTargetType.Blob)
                return null;

            var blob = (Blob)entry.Target;

            var accessor = GitCacheAccessor.Singleton(new BlameAccessor(_repoId, _repository, commit, path, CpToEncoding(commit.Encoding), _i18n.Value));
            var hunks = accessor.Result.Value;

            var model = new BlameModel
            {
                ReferenceName = referenceName,
                Sha = commit.Sha,
                Path = string.IsNullOrEmpty(path) ? "" : path,
                SizeString = FileHelper.GetSizeString(blob.Size),
                Brush = FileHelper.GetBrush(path),
                Hunks = hunks,
                BranchSelector = GetBranchSelectorModel(referenceName, commit.Sha, path),
                PathBar = new PathBarModel
                {
                    Name = Name,
                    Action = "Tree",
                    Path = path,
                    ReferenceName = referenceName,
                    ReferenceSha = commit.Sha,
                    HideLastSlash = true,
                },
            };

            return model;
        }

        public string GetArchiveFilename(string path, string newline, out string referenceName)
        {
            var commit = GetCommitByPath(ref path, out referenceName);
            if (commit == null)
                return null;

            if (referenceName == null)
                referenceName = commit.Sha;

            var accessor = GitCacheAccessor.Singleton(new ArchiverAccessor(_repoId, _repository, commit, newline, CpToEncoding(commit.Encoding), _i18n.Value));

            return accessor.Result.Value;
        }

        public TagsModel GetTags()
        {
            var model = new TagsModel
            {
                Tags = (from tag in _repository.Tags
                        let commit = (tag.IsAnnotated ? tag.Annotation.Target : tag.Target) as Commit
                        where commit != null
                        select new TagModel
                        {
                            ReferenceName = tag.FriendlyName,
                            Sha = tag.Target.Sha,
                            When = ((Commit)tag.Target).Author.When,
                            MessageShort = ((Commit)tag.Target).MessageShort.RepetitionIfEmpty(UnknowString),
                        })
                        .OrderByDescending(s => s.When)
                        .ToArray()
            };
            return model;
        }

        public BranchesModel GetBranches()
        {
            var head = _repository.Head;
            if (head.Tip == null)
                return new BranchesModel();

            var key = CalcBranchesKey();
            var accessor = GitCacheAccessor.Singleton(new HistoryDivergenceAccessor(_repoId, _repository, key));
            var aheadBehinds = accessor.Result.Value;
            var model = new BranchesModel
            {
                Commit = ToCommitModel(head.Tip, head.FriendlyName),
                AheadBehinds = aheadBehinds.Select(s => new AheadBehindModel
                {
                    Ahead = s.Ahead,
                    Behind = s.Behind,
                    Commit = new CommitModel
                    {
                        ReferenceName = s.Name,
                        Author = CreateSafeSignature(s.AuthorName, s.AuthorEmail, s.AuthorWhen),
                        Committer = CreateSafeSignature(s.CommitterName, s.CommitterEmail, s.CommitterWhen),
                    },
                }).ToArray(),
            };
            return model;
        }

        public void DeleteBranch(string branch)
        {
            _repository.Branches.Remove(branch);
        }

        public void DeleteTag(string tag)
        {
            _repository.Tags.Remove(tag);
        }

        public ContributorsModel GetContributors(string path)
        {
            string referenceName;
            var commit = GetCommitByPath(ref path, out referenceName);
            if (commit == null)
                return null;

            var contributorsAccessor = GitCacheAccessor.Singleton(new ContributorsAccessor(_repoId, _repository, commit));
            var contributors = contributorsAccessor.Result.Value;
            contributors.OrderedCommits = contributors.OrderedCommits
                .Take(UserConfiguration.Current.NumberOfRepositoryContributors)
                .ToArray();
            var statistics = new RepositoryStatisticsModel();
            statistics.Current = contributors;
            statistics.Current.Branch = referenceName;

            if (_repository.Head.Tip != commit)
            {
                contributorsAccessor = GitCacheAccessor.Singleton(new ContributorsAccessor(_repoId, _repository, _repository.Head.Tip));
                statistics.Default = contributorsAccessor.Result.Value;
                statistics.Default.Branch = _repository.Head.FriendlyName;
            }

            var key = CalcBranchesKey(true);
            var repositorySizeAccessor = GitCacheAccessor.Singleton(new RepositorySizeAccessor(_repoId, _repository, key));
            statistics.RepositorySize = repositorySizeAccessor.Result.Value;

            var model = new ContributorsModel
            {
                RepositoryName = Name,
                Statistics = statistics,
            };
            return model;
        }
        #endregion

        #region Repository Settings
        public string GetHeadBranch()
        {
            var head = _repository.Head;
            if (head == null)
                return null;
            return head.FriendlyName;
        }

        public string[] GetLocalBranches()
        {
            return _repository.Branches.Select(s => s.FriendlyName).OrderBy(s => s, new StringLogicalComparer()).ToArray();
        }

        public bool SetHeadBranch(string name)
        {
            var refs = _repository.Refs;
            var refer = refs["refs/heads/" + (name ?? "master")];
            if (refer == null)
                return false;
            refs.UpdateTarget(refs.Head, refer);
            return true;
        }
        #endregion

        #region Private Methods
        private BranchSelectorModel GetBranchSelectorModel(string referenceName, string refer, string path)
        {
            var model = new BranchSelectorModel
            {
                Branches = _repository.Branches.Select(s => s.FriendlyName).OrderBy(s => s, new StringLogicalComparer()).ToList(),
                Tags = _repository.Tags.Select(s => s.FriendlyName).OrderByDescending(s => s, new StringLogicalComparer()).ToList(),
                Current = referenceName ?? refer.ToShortSha(),
                Path = path,
            };
            model.CurrentIsBranch = model.Branches.Contains(referenceName) || !model.Tags.Contains(referenceName);

            return model;
        }

        private Commit GetCommitByPath(ref string path, out string referenceName)
        {
            referenceName = null;

            if (string.IsNullOrEmpty(path))
            {
                referenceName = _repository.Head.FriendlyName;
                path = "";
                return _repository.Head.Tip;
            }

            path = path + "/";
            var p = path;
            var branch = _repository.Branches.FirstOrDefault(s => p.StartsWith(s.FriendlyName + "/"));
            if (branch != null && branch.Tip != null)
            {
                referenceName = branch.FriendlyName;
                path = path.Substring(referenceName.Length).Trim('/');
                return branch.Tip;
            }

            var tag = _repository.Tags.FirstOrDefault(s => p.StartsWith(s.FriendlyName + "/"));
            if (tag != null && tag.Target is Commit)
            {
                referenceName = tag.FriendlyName;
                path = path.Substring(referenceName.Length).Trim('/');
                return (Commit)tag.Target;
            }

            var index = path.IndexOf('/');
            var commit = _repository.Lookup<Commit>(path.Substring(0, index));
            path = path.Substring(index + 1).Trim('/');
            return commit;
        }

        private CommitModel ToCommitModel(Commit commit, string referenceName, bool isTree = true, string detailFilter = null, Tree compareWith = null)
        {
            if (commit == null)
                return null;

            var model = new CommitModel
            {
                ReferenceName = referenceName,
                Sha = commit.Sha,
                CommitMessageShort = commit.MessageShort.RepetitionIfEmpty(UnknowString),
                CommitMessage = commit.Message.RepetitionIfEmpty(UnknowString),
                Author = commit.Author,
                Committer = commit.Committer,
                Parents = commit.Parents.Select(e => e.Sha).ToArray(),
            };
            if (detailFilter != null)
            {
                if (detailFilter != "" && isTree)
                    detailFilter = detailFilter + "/";
                var firstTree = compareWith != null
                    ? commit.Tree
                    : commit.Parents.Any()
                        ? commit.Parents.First().Tree
                        : null;
                if (compareWith == null)
                    compareWith = commit.Tree;
                var compareOptions = new LibGit2Sharp.CompareOptions
                {
                    Similarity = SimilarityOptions.Renames,
                };
                var paths = detailFilter == ""
                    ? null
                    : new[] { detailFilter };
                var changes = _repository.Diff.Compare<TreeChanges>(firstTree, compareWith, paths, compareOptions: compareOptions);
                var patches = _repository.Diff.Compare<Patch>(firstTree, compareWith, paths, compareOptions: compareOptions);
                model.Changes = (from s in changes
                                 where (s.Path.Replace('\\', '/') + '/').StartsWith(detailFilter)
                                 orderby s.Path
                                 let patch = patches[s.Path]
                                 select new CommitChangeModel
                                 {
                                     //Name = s.Name,
                                     OldPath = s.OldPath.Replace('\\', '/'),
                                     Path = s.Path.Replace('\\', '/'),
                                     ChangeKind = s.Status,
                                     LinesAdded = patch.LinesAdded,
                                     LinesDeleted = patch.LinesDeleted,
                                     Patch = patch.Patch,
                                 })
                                 .ToArray();
            }

            return model;
        }

        private Encoding CpToEncoding(string encoding)
        {
            try
            {
                if (encoding.StartsWith("cp", StringComparison.OrdinalIgnoreCase))
                    return Encoding.GetEncoding(int.Parse(encoding.Substring(2)));

                return Encoding.GetEncoding(encoding);
            }
            catch
            {
                return null;
            }
        }

        private string CalcBranchesKey(bool includeTags = false)
        {
            var sb = new StringBuilder();
            var head = _repository.Head;
            sb.Append(":");
            sb.Append(head.CanonicalName);
            if (head.Tip != null)
                sb.Append(head.Tip.Sha);
            sb.Append(';');
            foreach (var branch in _repository.Branches.OrderBy(s => s.CanonicalName))
            {
                sb.Append(':');
                sb.Append(branch.CanonicalName);
                if (branch.Tip != null)
                    sb.Append(branch.Tip.Sha);
            }
            if (includeTags)
            {
                sb.Append(';');
                foreach (var tag in _repository.Tags.OrderBy(s => s.CanonicalName))
                {
                    sb.Append(':');
                    sb.Append(tag.CanonicalName);
                    if (tag.Target != null)
                        sb.Append(tag.Target.Sha);
                }
            }
            return sb.ToString();
        }

        private Signature CreateSafeSignature(string name, string email, DateTimeOffset when)
        {
            return new Signature(name.RepetitionIfEmpty(UnknowString), email, when);
        }
        #endregion

        #region Static Methods
        public static bool CreateRepository(string name, string remoteUrl = null)
        {
            var path = Path.Combine(UserConfiguration.Current.RepositoryPath, name);
            try
            {
                using (var repo = new Repository(Repository.Init(path, true)))
                {
                    repo.Config.Set("core.logallrefupdates", true);
                    if (remoteUrl != null)
                    {
                        repo.Network.Remotes.Add("origin", remoteUrl, "+refs/*:refs/*");
                        Scheduler.Instance.AddJob(new SingleJob(() =>
                        {
                            using (var fetch_repo = new Repository(repo.Info.Path))
                            {
                                fetch_repo.Fetch("origin");
                            }
                        }));
                    }
                }
                return true;
            }
            catch
            {
                try
                {
                    Directory.Delete(path, true);
                }
                catch { }
                return false;
            }
        }

        public static bool DeleteRepository(string name)
        {
            var path = Path.Combine(UserConfiguration.Current.RepositoryPath, name);
            var temp = Path.Combine(UserConfiguration.Current.RepositoryPath, name + "." + DateTime.Now.Ticks + ".del");

            var retry = 3;
            for (; retry > 0; retry--)
                try
                {
                    Directory.Move(path, temp);
                    break;
                }
                catch
                {
                    Task.Delay(1000).Wait();
                }

            for (; retry > 0; retry--)
                try
                {
                    var di = new DirectoryInfo(temp);

                    foreach (var info in di.GetFileSystemInfos("*", SearchOption.AllDirectories))
                        info.Attributes = FileAttributes.Archive;

                    break;
                }
                catch
                {
                    Task.Delay(1000).Wait();
                }

            for (; retry > 0; retry--)
                try
                {
                    Directory.Delete(temp, true);
                    break;
                }
                catch
                {
                    Task.Delay(1000).Wait();
                }

            return retry > 0;
        }

        private static DirectoryInfo GetDirectoryInfo(string project)
        {
            return new DirectoryInfo(Path.Combine(UserConfiguration.Current.RepositoryPath, project));
        }
        #endregion

        #region RunGitCmd
        // un-safe implementation
        private void RunGitCmd(string serviceName, bool advertiseRefs, Stream inStream, Stream outStream)
        {
            var args = serviceName + " --stateless-rpc";
            if (advertiseRefs)
                args += " --advertise-refs";
            args += " \"" + _repositoryPath + "\"";

            var info = new System.Diagnostics.ProcessStartInfo(Path.Combine(UserConfiguration.Current.GitCorePath, "git.exe"), args)
            {
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(UserConfiguration.Current.RepositoryPath),
            };

            using (var process = System.Diagnostics.Process.Start(info))
            {
                inStream.CopyTo(process.StandardInput.BaseStream);
                process.StandardInput.Close();
                process.StandardOutput.BaseStream.CopyTo(outStream);

                process.WaitForExit();
            }
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_repository != null)
                    {
                        _repository.Dispose();
                    }
                }

                _disposed = true;
            }
        }

        ~GitService()
        {
            Dispose(false);
        }
        #endregion
    }
}
