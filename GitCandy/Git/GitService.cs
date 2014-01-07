using GitCandy.Base;
using GitCandy.Configuration;
using GitCandy.Extensions;
using GitCandy.Models;
using ICSharpCode.SharpZipLib.Zip;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitCandy.Git
{
    public class GitService : IDisposable
    {
        private const string NoCommitMessage = "<+++++>";

        private readonly Repository _repository;
        private readonly string _repositoryPath;
        private readonly Lazy<Encoding> _i18n;
        private bool _disposed;

        public Encoding I18n { get { return _i18n.Value; } }
        public string Name { get; private set; }

        public GitService(string path)
        {
            if (!Repository.IsValid(path))
                throw new RepositoryNotFoundException(String.Format(CultureInfo.InvariantCulture, "Path '{0}' doesn't point at a valid Git repository or workdir.", path));

            _repositoryPath = path;
            _repository = new Repository(path);
            _i18n = new Lazy<Encoding>(() =>
            {
                var entry = _repository.Config.Get<string>("i18n.commitEncoding");
                return entry == null
                    ? null
                    : CpToEncoding(entry.Value);
            });
            Name = new DirectoryInfo(path).Name;
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
            if (_repository.Head == null || _repository.Head.Tip == null) // empty repository
                return new TreeModel
                {
                    ReferenceName = "HEAD",
                };

            string referenceName;
            var commit = GetCommitByPath(ref path, out referenceName);
            if (commit == null)
                return null;

            var model = new TreeModel
            {
                ReferenceName = referenceName,
                Path = string.IsNullOrEmpty(path) ? "" : path,
                Commit = new CommitModel
                {
                    Sha = commit.Sha,
                    Author = commit.Author,
                    Committer = commit.Committer,
                    CommitMessageShort = commit.MessageShort.RepetitionIfEmpty(NoCommitMessage),
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

            IEnumerable<Commit> ancestors = _repository.Commits
                .QueryBy(new CommitFilter { Since = commit, SortBy = CommitSortStrategies.Topological });

            var cacheItem = GitCache.Get<TreeCacheItem>(tree.Sha, "tree");
            var missing = cacheItem == null;
            if (missing)
            {
                cacheItem = new TreeCacheItem();
                if (model.IsRoot)
                {
                    ancestors = ancestors.ToList();
                    cacheItem.CommitsCount = ancestors.Count();
                    cacheItem.ContributorsCount = ancestors.GroupBy(s => s.Author.Name + "!" + s.Author.Email).Count();
                }
            }
            var entries = CalculateRevisionSummary(referenceName, tree, ancestors, ref cacheItem.RevisionSummary);
            if (missing)
            {
                GitCache.Set(tree.Sha, "tree", cacheItem);
            }

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
                model.Scope = new RepositoryScope
                {
                    Commits = cacheItem.CommitsCount,
                    Branches = _repository.Branches.Count(),
                    Tags = _repository.Tags.Count(),
                    Contributors = cacheItem.ContributorsCount,
                };

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
                    CommitMessage = commit.Message.RepetitionIfEmpty(NoCommitMessage),
                    CommitMessageShort = commit.MessageShort.RepetitionIfEmpty(NoCommitMessage),
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

        public CommitsModel GetCommits(string path, int page = 1, int pagesize = 20)
        {
            string referenceName;
            var commit = GetCommitByPath(ref path, out referenceName);
            if (commit == null)
                return null;

            if (!string.IsNullOrEmpty(path) && commit[path] == null)
                return null;

            var commits = GitCache.Get<RevisionSummaryCacheItem[]>((commit.Sha + path).CalcSha(), "commits");
            if (commits == null)
            {
                var ancestors = _repository.Commits
                    .QueryBy(new CommitFilter { Since = commit, SortBy = CommitSortStrategies.Topological | CommitSortStrategies.Time })
                    .PathFilter(path)
                    .ToList();

                commits = ancestors.Select(s => new RevisionSummaryCacheItem
                {
                    Sha = s.Sha,
                    MessageShort = s.MessageShort.RepetitionIfEmpty(NoCommitMessage),
                    AuthorName = s.Author.Name,
                    AuthorEmail = s.Author.Email,
                    AuthorWhen = s.Author.When,
                    CommitterName = s.Committer.Name,
                    CommitterEmail = s.Committer.Email,
                    CommitterWhen = s.Committer.When,
                }).ToArray();

                GitCache.Set((commit.Sha + path).CalcSha(), "commits", commits);
            }

            var model = new CommitsModel
            {
                ReferenceName = referenceName,
                Sha = commit.Sha,
                Commits = commits
                    .Skip((page - 1) * pagesize)
                    .Take(pagesize)
                    .Select(s => new CommitModel
                    {
                        CommitMessageShort = s.MessageShort,
                        Sha = s.Sha,
                        Author = new Signature(s.AuthorName, s.AuthorEmail, s.AuthorWhen),
                        Committer = new Signature(s.CommitterName, s.CommitterEmail, s.CommitterWhen),
                    })
                    .ToList(),
                CurrentPage = page,
                ItemCount = commits.Count(),
                Path = string.IsNullOrEmpty(path) ? "" : path,
                PathBar = new PathBarModel
                {
                    Name = Name,
                    Action = "Commits",
                    Path = path,
                    ReferenceName = referenceName,
                    ReferenceSha = commit.Sha,
                    HideLastSlash = path != "" && commit[path].TargetType == TreeEntryTargetType.Blob,
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
            var bytes = blob.GetContentStream().ToBytes();
            var encoding = FileHelper.DetectEncoding(bytes, CpToEncoding(commit.Encoding), _i18n.Value);
            if (encoding == null)
                return null;

            var code = FileHelper.ReadToEnd(bytes, encoding);
            var reader = new StringReader(code);

            var hunks = GitCache.Get<BlameHunkModel[]>(blob.Sha, "blame");
            if (hunks == null)
            {
                var blame = _repository.Blame(path, new BlameOptions { StartingAt = commit });
                hunks = blame.Select(s => new BlameHunkModel
                {
                    Code = reader.ReadLines(s.LineCount),
                    StartLine = s.FinalStartLineNumber,
                    EndLine = s.LineCount,
                    MessageShort = s.FinalCommit.MessageShort,
                    Sha = s.FinalCommit.Sha,
                    Author = s.FinalCommit.Author.Name,
                    AuthorEmail = s.FinalCommit.Author.Email,
                    AuthorDate = s.FinalCommit.Author.When,
                })
                .ToArray();
                GitCache.Set(blob.Sha, "blame", hunks);
            }

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

            var key = "archive";
            if (newline != null)
                key += newline.GetHashCode().ToString("x");
            bool exist;
            var filename = GitCache.GetCacheFilename(commit.Sha, key, out exist, true);
            if (exist)
                return filename;

            using (var zipOutputStream = new ZipOutputStream(new FileStream(filename, FileMode.Create)))
            {
                var stack = new Stack<Tree>();

                stack.Push(commit.Tree);
                while (stack.Count != 0)
                {
                    var tree = stack.Pop();
                    foreach (var entry in tree)
                    {
                        byte[] bytes;
                        switch (entry.TargetType)
                        {
                            case TreeEntryTargetType.Blob:
                                zipOutputStream.PutNextEntry(new ZipEntry(entry.Path));
                                var blob = (Blob)entry.Target;
                                bytes = blob.GetContentStream().ToBytes();
                                if (newline == null)
                                    zipOutputStream.Write(bytes, 0, bytes.Length);
                                else
                                {
                                    var encoding = FileHelper.DetectEncoding(bytes, CpToEncoding(commit.Encoding), _i18n.Value);
                                    if (encoding == null)
                                        zipOutputStream.Write(bytes, 0, bytes.Length);
                                    else
                                    {
                                        bytes = FileHelper.ReplaceNewline(bytes, encoding, newline);
                                        zipOutputStream.Write(bytes, 0, bytes.Length);
                                    }
                                }
                                break;
                            case TreeEntryTargetType.Tree:
                                stack.Push((Tree)entry.Target);
                                break;
                            case TreeEntryTargetType.GitLink:
                                zipOutputStream.PutNextEntry(new ZipEntry(entry.Path + "/.gitsubmodule"));
                                bytes = Encoding.ASCII.GetBytes(entry.Target.Sha);
                                zipOutputStream.Write(bytes, 0, bytes.Length);
                                break;
                        }
                    }
                }
                zipOutputStream.SetComment(commit.Sha);
            }

            return filename;
        }

        public TagsModel GetTags()
        {
            var model = new TagsModel
            {
                Tags = _repository.Tags.Select(s => new TagModel
                {
                    ReferenceName = s.Name,
                    Sha = s.Target.Sha,
                    When = ((Commit)s.Target).Author.When,
                    MessageShort = ((Commit)s.Target).MessageShort.RepetitionIfEmpty(NoCommitMessage),
                })
                .OrderByDescending(s => s.When)
                .ToArray()
            };
            return model;
        }

        public BranchesModel GetBranches()
        {
            var head = _repository.Head;
            var sha = CalcBranchesSha();
            var aheadBehinds = GitCache.Get<RevisionSummaryCacheItem[]>(sha, "branches");
            if (aheadBehinds == null)
            {
                aheadBehinds = _repository.Branches
                    .Where(s => s != head && s.Name != "HEAD")
                    .OrderByDescending(s => s.Tip.Author.When)
                    .Select(branch =>
                    {
                        var commit = branch.Tip;
                        var divergence = _repository.ObjectDatabase.CalculateHistoryDivergence(commit, head.Tip);
                        return new RevisionSummaryCacheItem
                        {
                            Ahead = divergence.AheadBy ?? 0,
                            Behind = divergence.BehindBy ?? 0,
                            Name = branch.Name,
                            Sha = commit.Sha,
                            AuthorName = commit.Author.Name,
                            AuthorEmail = commit.Author.Email,
                            AuthorWhen = commit.Author.When,
                            CommitterName = commit.Committer.Name,
                            CommitterEmail = commit.Committer.Email,
                            CommitterWhen = commit.Committer.When,
                        };
                    })
                    .ToArray();
                GitCache.Set(sha, "branches", aheadBehinds);
            }
            var model = new BranchesModel
            {
                Commit = ToCommitModel(head.Tip, head.Name),
                AheadBehinds = aheadBehinds.Select(s => new AheadBehindModel
                {
                    Ahead = s.Ahead,
                    Behind = s.Behind,
                    Commit = new CommitModel
                    {
                        ReferenceName = s.Name,
                        Author = new Signature(s.AuthorName, s.AuthorEmail, s.AuthorWhen),
                        Committer = new Signature(s.CommitterName, s.CommitterEmail, s.CommitterWhen),
                    },
                }).ToArray(),
            };
            return model;
        }

        public ContributorsModel GetContributors(string path)
        {
            string referenceName;
            var commit = GetCommitByPath(ref path, out referenceName);
            if (commit == null)
                return null;

            var ancestors = _repository.Commits
              .QueryBy(new CommitFilter { Since = commit });

            var contributors = GitCache.Get<ContributorCommitsModel[]>(commit.Sha, "contributors");
            if (contributors == null)
            {
                contributors = ancestors.GroupBy(s => s.Author.ToString())
                    .Select(s => new ContributorCommitsModel
                    {
                        Author = s.Key,
                        CommitsCount = s.Count(),
                    })
                    .OrderByDescending(s => s.CommitsCount)
                    .ThenBy(s => s.Author, new StringLogicalComparer())
                    .ToArray();
                GitCache.Set(commit.Sha, "contributors", contributors);
            }

            var statistics = new RepositoryStatisticsModel();
            var stats = GitCache.Get<RepositoryStatisticsModel.Statistics>(commit.Sha, "statistics");
            int size;
            if (stats == null)
            {
                stats = new RepositoryStatisticsModel.Statistics
                {
                    Branch = referenceName,
                    Commits = ancestors.Count(),
                    Contributors = ancestors.GroupBy(s => s.Author.ToString()).Count(),
                    Files = FilesInCommit(commit, out size),
                    SourceSize = size,
                };

                GitCache.Set(commit.Sha, "statistics", stats);
            }
            statistics.Current = stats;

            if (_repository.Head.Tip != commit)
            {
                commit = _repository.Head.Tip;
                stats = GitCache.Get<RepositoryStatisticsModel.Statistics>(commit.Sha, "statistics");
                if (stats == null)
                {
                    ancestors = _repository.Commits.QueryBy(new CommitFilter { Since = commit });
                    stats = new RepositoryStatisticsModel.Statistics
                    {
                        Branch = _repository.Head.Name,
                        Commits = ancestors.Count(),
                        Contributors = ancestors.GroupBy(s => s.Author.ToString()).Count(),
                        Files = FilesInCommit(commit, out size),
                        SourceSize = size,
                    };

                    GitCache.Set(commit.Sha, "statistics", stats);
                }
                statistics.Default = stats;
            }

            var sha = CalcBranchesSha(true);
            var sizeOfRepo = GitCache.Get<long>(sha, "size");
            if (sizeOfRepo == 0)
            {
                sizeOfRepo = SizeOfRepository();
                GitCache.Set(sha, "size", sizeOfRepo);
            }
            statistics.RepositorySize = sizeOfRepo;

            var model = new ContributorsModel
            {
                RepositoryName = Name,
                Contributors = contributors,
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
            return head.Name;
        }

        public string[] GetLocalBranches()
        {
            return _repository.Branches.Select(s => s.Name).OrderBy(s => s, new StringLogicalComparer()).ToArray();
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
        private List<TreeEntryModel> CalculateRevisionSummary(string referenceName, Tree tree, IEnumerable<Commit> ancestors, ref RevisionSummaryCacheItem[] cache)
        {
            var entries = tree
                .OrderBy(s => s.TargetType == TreeEntryTargetType.Blob)
                .ThenBy(s => s.Name, new StringLogicalComparer())
                .Select(s => new TreeEntryModel
                {
                    Name = s.Name,
                    ReferenceName = referenceName,
                    Path = s.Path.Replace('\\', '/'),
                    Commit = new CommitModel(),
                    Sha = s.Target.Sha,
                    EntryType = s.TargetType,
                })
                .ToList();

            if (cache != null)
            {
                foreach (var entry in entries)
                {
                    var commitModel = entry.Commit;
                    var item = cache.First(s => s.Name == entry.Name);
                    commitModel.Sha = item.Sha;
                    commitModel.CommitMessageShort = item.MessageShort;
                    commitModel.Author = new Signature(item.AuthorName, item.AuthorEmail, item.AuthorWhen);
                    commitModel.Committer = new Signature(item.CommitterName, item.CommitterEmail, item.CommitterWhen);
                }
                return entries;
            }

            // null, continue search current reference
            // true, have found, done
            // false, search has been interrupted, but waiting for next match
            var status = new bool?[entries.Count];
            var done = entries.Count;
            Commit lastCommit = null;
            foreach (var ancestor in ancestors)
            {
                for (var index = 0; index < entries.Count; index++)
                {
                    if (status[index] == true)
                        continue;
                    var entryModel = entries[index];
                    var ancestorEntry = ancestor[entryModel.Path];
                    if (ancestorEntry != null && ancestorEntry.Target.Sha == entryModel.Sha)
                    {
                        var commitModel = entryModel.Commit;
                        commitModel.Sha = ancestor.Sha;
                        commitModel.CommitMessageShort = ancestor.MessageShort.RepetitionIfEmpty(NoCommitMessage);
                        commitModel.Author = ancestor.Author;
                        commitModel.Committer = ancestor.Committer;

                        status[index] = null;
                    }
                    else if (status[index] == null)
                    {
                        var over = true;
                        foreach (var parent in lastCommit.Parents) // Backtracking
                        {
                            if (parent.Sha == ancestor.Sha)
                                continue;
                            var entry = parent[entryModel.Path];
                            if (entry != null && entry.Target.Sha == entryModel.Sha)
                            {
                                over = false;
                                break;
                            }
                        }
                        status[index] = over;
                        if (over)
                            done--;
                    }
                }
                if (done == 0)
                    break;
                lastCommit = ancestor;
            }

            cache = entries.Select(s => new RevisionSummaryCacheItem
            {
                Name = s.Name,
                Sha = s.Commit.Sha,
                MessageShort = s.Commit.CommitMessageShort,
                AuthorName = s.Commit.Author.Name,
                AuthorEmail = s.Commit.Author.Email,
                AuthorWhen = s.Commit.Author.When,
                CommitterName = s.Commit.Committer.Name,
                CommitterEmail = s.Commit.Committer.Email,
                CommitterWhen = s.Commit.Committer.When,
            }).ToArray();

            return entries;
        }

        private BranchSelectorModel GetBranchSelectorModel(string referenceName, string refer, string path)
        {
            var model = new BranchSelectorModel
            {
                Branches = _repository.Branches.Select(s => s.Name).OrderBy(s => s, new StringLogicalComparer()).ToList(),
                Tags = _repository.Tags.Select(s => s.Name).OrderByDescending(s => s, new StringLogicalComparer()).ToList(),
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
                referenceName = _repository.Head.Name;
                path = "";
                return _repository.Head.Tip;
            }

            path = path + "/";
            var p = path;
            var branch = _repository.Branches.FirstOrDefault(s => p.StartsWith(s.Name + "/"));
            if (branch != null && branch.Tip != null)
            {
                referenceName = branch.Name;
                path = path.Substring(referenceName.Length).Trim('/');
                return branch.Tip;
            }

            var tag = _repository.Tags.FirstOrDefault(s => p.StartsWith(s.Name + "/"));
            if (tag != null && tag.Target is Commit)
            {
                referenceName = tag.Name;
                path = path.Substring(referenceName.Length).Trim('/');
                return (Commit)tag.Target;
            }

            var index = path.IndexOf('/');
            var commit = _repository.Lookup<Commit>(path.Substring(0, index));
            path = path.Substring(index + 1).Trim('/');
            return commit;
        }

        private CommitModel ToCommitModel(Commit commit, string referenceName, bool isTree = true, string detailFilter = null)
        {
            if (commit == null)
                return null;

            var model = new CommitModel
            {
                ReferenceName = referenceName,
                Sha = commit.Sha,
                CommitMessageShort = commit.MessageShort.RepetitionIfEmpty(NoCommitMessage),
                CommitMessage = commit.Message.RepetitionIfEmpty(NoCommitMessage),
                Author = commit.Author,
                Committer = commit.Committer,
                Parents = commit.Parents.Select(e => e.Sha).ToArray(),
            };
            if (detailFilter != null)
            {
                if (detailFilter != "" && isTree)
                    detailFilter = detailFilter + "/";
                var firstTree = commit.Parents.Any()
                    ? commit.Parents.First().Tree
                    : null;
                var secondTree = commit.Tree;
                var compareOptions = new LibGit2Sharp.CompareOptions
                {
                    Similarity = SimilarityOptions.Renames,
                };
                var paths = detailFilter == ""
                    ? null
                    : new[] { detailFilter };
                var changes = _repository.Diff.Compare<TreeChanges>(firstTree, secondTree, paths, compareOptions: compareOptions);
                var patches = _repository.Diff.Compare<Patch>(firstTree, secondTree, paths, compareOptions: compareOptions);
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

        private string CalcBranchesSha(bool includeTags = false)
        {
            var sb = new StringBuilder();
            sb.Append(":HEAD");
            if (_repository.Head.Tip != null)
                sb.Append(_repository.Head.Tip.Sha);
            sb.Append(';');
            foreach (var branch in _repository.Branches.OrderBy(s => s.Name))
            {
                sb.Append(':');
                sb.Append(branch.Name);
                if (branch.Tip != null)
                    sb.Append(branch.Tip.Sha);
            }
            sb.Append(';');
            if (includeTags)
            {
                foreach (var tag in _repository.Tags.OrderBy(s => s.Name))
                {
                    sb.Append(':');
                    sb.Append(tag.Name);
                    if (tag.Target != null)
                        sb.Append(tag.Target.Sha);
                }
            }
            return sb.ToString().CalcSha();
        }

        private int FilesInCommit(Commit commit, out int sourceSize)
        {
            var count = 0;
            var stack = new Stack<Tree>();
            sourceSize = 0;

            stack.Push(commit.Tree);
            while (stack.Count != 0)
            {
                var tree = stack.Pop();
                foreach (var entry in tree)
                    switch (entry.TargetType)
                    {
                        case TreeEntryTargetType.Blob:
                            count++;
                            sourceSize += ((Blob)entry.Target).Size;
                            break;
                        case TreeEntryTargetType.Tree:
                            stack.Push((Tree)entry.Target);
                            break;
                    }
            }
            return count;
        }

        private long SizeOfRepository()
        {
            var size = 0L;
            var info = new DirectoryInfo(_repositoryPath);
            foreach (var file in info.GetFiles("*", SearchOption.AllDirectories))
            {
                size += file.Length;
            }
            return size;
        }
        #endregion

        #region Static Methods
        public static bool CreateRepository(string name)
        {
            var path = Path.Combine(UserConfiguration.Current.RepositoryPath, name);
            try
            {
                Repository.Init(path, true);
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

        public static DirectoryInfo GetDirectoryInfo(string project)
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

            var info = new System.Diagnostics.ProcessStartInfo(UserConfiguration.Current.GitExePath, args)
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
                process.StandardInput.Write('\0');
                process.StandardOutput.BaseStream.CopyTo(outStream);

                process.WaitForExit();
            }
        }

        public static bool VerifyGit(string path)
        {
            var args = "--version";

            var info = new System.Diagnostics.ProcessStartInfo(path, args)
            {
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(UserConfiguration.Current.RepositoryPath),
            };

            try
            {
                using (var process = System.Diagnostics.Process.Start(info))
                {
                    var output = process.StandardOutput.ReadToEnd();
                    return output.StartsWith("git version");
                }
            }
            catch
            {
                return false;
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