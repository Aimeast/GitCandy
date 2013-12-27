using System;

namespace GitCandy.Models
{
    [Serializable]
    public class ContributorCommitsModel
    {
        public string Author { get; set; }
        public int CommitsCount { get; set; }
    }
}