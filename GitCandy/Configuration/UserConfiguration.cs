
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

        public string RepositoryPath { get; set; }

        public string CachePath { get; set; }

        public string GitExePath { get; set; }

        [RecommendedValue(30)]
        public int NumberOfCommitsPerPage { get; set; }

        [RecommendedValue(30)]
        public int NumberOfItemsPerList { get; set; }
    }
}