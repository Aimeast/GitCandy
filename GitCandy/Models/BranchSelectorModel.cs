using System.Collections.Generic;

namespace GitCandy.Models
{
    public class BranchSelectorModel
    {
        public IEnumerable<string> Branches { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public string Current { get; set; }
        public bool CurrentIsBranch { get; set; }
        public string Path { get; set; }
    }
}