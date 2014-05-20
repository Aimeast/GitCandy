using System;

namespace GitCandy.Models
{
    [Serializable]
    public class BlameHunkModel
    {
        public string Code { get; set; }
        public string MessageShort { get; set; }
        public string Sha { get; set; }
        public string Author { get; set; }
        public string AuthorEmail { get; set; }
        public DateTimeOffset AuthorDate { get; set; }
    }
}