
namespace GitCandy.Models
{
    public class ContributorsModel : RepositoryModelBase
    {
        public ContributorCommitsModel[] Contributors { get; set; }
        public RepositoryStatisticsModel Statistics { get; set; }
    }
}