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

        public override bool IsAsync { get { return false; } }

        protected override string GetCacheKey()
        {
            return GetCacheKey(key);
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
    }
}
