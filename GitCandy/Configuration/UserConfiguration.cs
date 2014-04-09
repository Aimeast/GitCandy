
using GitCandy.Git;
using System;
using System.Web;
namespace GitCandy.Configuration
{
    [ConfigurationKey("UserConfiguration")]
    public class UserConfiguration : ConfigurationEntry<UserConfiguration>
    {
        [RecommendedValue(true)]
        public bool IsPublicServer { get; set; }

        [RecommendedValue(true, defaultValue: false)]
        public bool ForceSsl { get; set; }

        [RecommendedValue(443)]
        public int SslPort { get; set; }

        [RecommendedValue(true)]
        public bool LocalSkipCustomError { get; set; }

        [RecommendedValue(true)]
        public bool AllowRegisterUser { get; set; }

        [RecommendedValue(true)]
        public bool AllowRepositoryCreation { get; set; }

        private string _RepositoryPath;
        public string RepositoryPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_RepositoryPath))
                {
                    _RepositoryPath = HttpContext.Current.Server.MapPath("~/App_Data/Repositories/");
                }
                return _RepositoryPath;
            }
            set
            {
                _RepositoryPath = value;
            }
        }

        private string _cachePath;
        public string CachePath 
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_cachePath))
                {
                    _cachePath = HttpContext.Current.Server.MapPath("~/App_Data/Cache/");
                }
                return _cachePath;
            }
            set
            {
                _cachePath = value;
            }
        }

        private string _GitExePath;
        public string GitExePath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_GitExePath))
                {
                    _GitExePath = @"C:\Program Files (x86)\Git\bin\git.exe";
                    if(!GitService.VerifyGit(_GitExePath))
                    {
                        _GitExePath = @"C:\Program Files\Git\bin\git.exe";
                        if (!GitService.VerifyGit(_GitExePath))
                        {
                            throw new Exception("Please Config the GitExePath first.");
                        }
                    }
                }
                return _GitExePath;
            }
            set
            {
                _GitExePath = value;
            }
        }

        [RecommendedValue(30)]
        public int NumberOfCommitsPerPage { get; set; }

        [RecommendedValue(30)]
        public int NumberOfItemsPerList { get; set; }
    }
}