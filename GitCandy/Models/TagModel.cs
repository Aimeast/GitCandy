using System;

namespace GitCandy.Models
{
    public class TagModel
    {
        public string ReferenceName { get; set; }
        public string Sha { get; set; }
        public DateTime DateTime { get; set; }
        public string MessageShort { get; set; }
    }
}