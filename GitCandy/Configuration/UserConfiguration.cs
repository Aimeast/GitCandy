using System.Collections.Generic;

namespace GitCandy.Configuration
{
    [ConfigurationKey("UserConfiguration")]
    public class UserConfiguration : ConfigurationEntry<UserConfiguration>
    {
        public UserConfiguration()
        {
            HostKeys = new List<HostKey>();
        }

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

        public string GitCorePath { get; set; }

        [RecommendedValue(30)]
        public int NumberOfCommitsPerPage { get; set; }

        [RecommendedValue(30)]
        public int NumberOfItemsPerList { get; set; }

        [RecommendedValue(50)]
        public int NumberOfRepositoryContributors { get; set; }

        [RecommendedValue(22)]
        public int SshPort { get; set; }

        [RecommendedValue(true)]
        public bool EnableSsh { get; set; }

        public List<HostKey> HostKeys { get; set; }
    }
}
