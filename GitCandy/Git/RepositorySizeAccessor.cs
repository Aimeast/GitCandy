using GitCandy.Git.Cache;
using LibGit2Sharp;
using System.Diagnostics.Contracts;
using System.IO;

namespace GitCandy.Git
{
    public class RepositorySizeAccessor : GitCacheAccessor<long, RepositorySizeAccessor>
    {
        private string key;

        public RepositorySizeAccessor(string repoId, Repository repo, string key)
            : base(repoId, repo)
        {
            Contract.Requires(key != null);

            this.key = key;
        }

        protected override string GetCacheFile()
        {
            return GetCacheFile(key);
        }

        protected override void Init()
        {
            result = 0;
        }

        protected override void Calculate()
        {
            var info = new DirectoryInfo(this.repoPath);
            foreach (var file in info.GetFiles("*", SearchOption.AllDirectories))
            {
                result += file.Length;
            }
            resultDone = true;
        }

        public override bool Equals(object obj)
        {
            var accessor = obj as RepositorySizeAccessor;
            return accessor != null
                && repoId == accessor.repoId
                && key == accessor.key;
        }

        public override int GetHashCode()
        {
            return typeof(RepositorySizeAccessor).GetHashCode() ^ (repoId + key).GetHashCode();
        }
    }
}
