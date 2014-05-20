using GitCandy.Configuration;
using GitCandy.Extensions;
using GitCandy.Log;
using GitCandy.Schedules;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace GitCandy.Git.Cache
{
    public abstract class GitCacheAccessor
    {
        protected static readonly Type[] accessors;
        protected static readonly object locker = new object();
        protected static readonly List<GitCacheAccessor> runningList = new List<GitCacheAccessor>();

        protected Task task;

        static GitCacheAccessor()
        {
            accessors = new[]
            {
                typeof(ArchiverAccessor),
                typeof(BlameAccessor),
                typeof(CommitsAccessor),
                typeof(ContributorsAccessor),
                typeof(HistoryDivergenceAccessor),
                typeof(LastCommitAccessor),
                typeof(RepositorySizeAccessor),
                typeof(ScopeAccessor),
                typeof(SummaryAccessor),
            };
        }

        public static T Singleton<T>(T accessor)
            where T : GitCacheAccessor
        {
            Contract.Requires(accessor != null);

            lock (locker)
            {
                var running = runningList.OfType<T>().FirstOrDefault(s => s == accessor);
                if (running == null)
                {
                    runningList.Add(accessor);
                    accessor.Init();
                    accessor.LoadOrCalculate();
                    return accessor;
                }
                return running;
            }
        }

        public static void Initialize()
        {
            var cachePath = UserConfiguration.Current.CachePath;
            DirectoryInfo info = new DirectoryInfo(cachePath);
            if (!info.Exists)
                info.Create();

            var path = typeof(GitCacheAccessor).Assembly.Location;
            var md5 = new MD5CryptoServiceProvider();
            var data = md5.ComputeHash(File.ReadAllBytes(path));
            var hash = data.BytesToString();

            var filename = Path.Combine(cachePath, "version");
            if (!File.Exists(filename) || File.ReadAllText(filename) != hash)
            {
                foreach (var dir in info.GetDirectories())
                    dir.Delete(true);
                foreach (var file in info.GetFiles())
                    file.Delete();

                File.WriteAllText(filename, hash);
            }
        }

        protected void RemoveFromRunningPool()
        {
            lock (locker)
            {
                runningList.Remove(this);
            }
        }

        protected abstract void Init();

        protected virtual void LoadOrCalculate()
        {
            var loaded = Load();
            task = loaded
                ? Task.Run(() => { })
                : new Task(() =>
                {
                    try
                    {
                        Calculate();
                        Save();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("GitCacheAccessor {0} exception" + Environment.NewLine + "{1}", this.GetType().FullName, ex);
                    }
                });

            if (!loaded)
            {
                if (IsAsync)
                {
                    Scheduler.Instance.AddJob(new SingleJob(task));
                }
                else
                {
                    task.Start();
                }
            }

            task.ContinueWith(t =>
            {
                Task.Delay(TimeSpan.FromMinutes(1.0)).Wait();
                RemoveFromRunningPool();
            });
        }

        protected abstract bool Load();

        protected abstract void Save();

        protected abstract void Calculate();

        public virtual bool IsAsync { get { return true; } }

        public static bool operator ==(GitCacheAccessor left, GitCacheAccessor right)
        {
            return object.ReferenceEquals(left, right)
                || !object.ReferenceEquals(left, null) && left.Equals(right)
                || !object.ReferenceEquals(right, null) && right.Equals(left);
        }

        public static bool operator !=(GitCacheAccessor left, GitCacheAccessor right)
        {
            return !object.ReferenceEquals(left, null) && !left.Equals(right)
                || !object.ReferenceEquals(right, null) && !right.Equals(left);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public abstract class GitCacheAccessor<TReturn, TAccessor> : GitCacheAccessor
        where TAccessor : GitCacheAccessor<TReturn, TAccessor>
    {
        public static int AccessorId { get; private set; }

        protected readonly string repoId;
        protected readonly Repository repo;
        protected readonly string repoPath;

        protected TReturn result;
        protected bool resultDone;

        public GitCacheReturn<TReturn> Result
        {
            get
            {
                if (task != null && !IsAsync)
                    task.Wait();
                return new GitCacheReturn<TReturn> { Value = result, Done = resultDone };
            }
        }

        static GitCacheAccessor()
        {
            var selfType = typeof(TAccessor);
            for (int i = 0; i < accessors.Length; i++)
            {
                if (accessors[i] == selfType)
                {
                    AccessorId = i + 1;
                    break;
                }
            }
            if (AccessorId == 0)
            {
                AccessorId = selfType.GetHashCode() % 10000000 + 10000000;
                Logger.Error("Not found the register of type '{0}', assign id {1} to '{2}'", selfType.FullName, AccessorId, selfType.Name);
            }
        }

        public GitCacheAccessor(string repoId, Repository repo)
        {
            Contract.Requires(repoId != null);
            Contract.Requires(repo != null);

            this.repoId = repoId;
            this.repo = repo;
            this.repoPath = repo.Info.Path;
        }

        protected abstract string GetCacheFile();

        protected virtual string GetCacheFile(params object[] keys)
        {
            Contract.Requires(keys != null);
            Contract.Requires(keys.Length > 0);
            Contract.Requires(keys.Select(s => s.SafyToString())
                .All(s => string.IsNullOrEmpty(s) || s.All(c => c != '\\' && c != '/')));

            var str = AccessorId + "\\" + repoId + "\\" + keys[0];
            for (int i = 1; i < keys.Length; i++)
            {
                var one = keys[i].SafyToString();
                if (!string.IsNullOrEmpty(one))
                    str += "-" + one;
            }
            return str;
        }

        protected override bool Load()
        {
            var filename = Path.Combine(UserConfiguration.Current.CachePath, GetCacheFile());
            if (File.Exists(filename))
            {
                try
                {
                    using (var fs = File.Open(filename, FileMode.Open))
                    {
                        var formatter = new BinaryFormatter();
                        var value = formatter.Deserialize(fs);
                        if (value is TReturn)
                        {
                            result = (TReturn)value;
                            resultDone = true;
                            return true;
                        }
                    }
                }
                catch { }
            }
            return false;
        }

        protected override void Save()
        {
            if (!resultDone)
                return;

            var info = new FileInfo(Path.Combine(UserConfiguration.Current.CachePath, GetCacheFile()));
            if (!info.Directory.Exists)
                info.Directory.Create();

            using (var fs = info.Create())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(fs, result);
                fs.Flush();
            }
        }
    }
}
