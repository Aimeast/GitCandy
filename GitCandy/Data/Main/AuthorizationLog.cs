using LiteDB;
using System;

namespace GitCandy.Data.Main
{
    public class AuthorizationLog
    {
        [BsonId]
        public Guid AuthCode { get; set; }
        public long UserID { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime Expires { get; set; }
        public string IssueIp { get; set; }
        public string LastIp { get; set; }
        public bool IsValid { get; set; }
        public virtual User User { get; set; }
    }
}
