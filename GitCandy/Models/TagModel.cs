using System;

namespace GitCandy.Models
{
    public class TagModel
    {
        public string ReferenceName { get; set; }
        public string Sha { get; set; }
        public DateTimeOffset When { get; set; }
        public string MessageShort { get; set; }
    }
}