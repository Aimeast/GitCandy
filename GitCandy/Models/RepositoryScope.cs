using System;

namespace GitCandy.Models
{
    [Serializable]
    public class RepositoryScope
    {
        public int Commits { get; set; }
        public int Branches { get; set; }
        public int Tags { get; set; }
        public int Contributors { get; set; }
    }
}