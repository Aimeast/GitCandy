using System;

namespace GitCandy.Data
{
    public class MigrationHistory
    {
        public int Id { get; set; }
        public int Version { get; set; }
        public DateTime MigrationDateTime { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
