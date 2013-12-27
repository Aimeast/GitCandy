using System;
using System.Collections.Generic;

namespace GitCandy.DAL
{
    public partial class AuthorizationLog
    {
        public System.Guid AuthCode { get; set; }
        public long UserID { get; set; }
        public System.DateTime IssueDate { get; set; }
        public System.DateTime Expires { get; set; }
        public string IssueIp { get; set; }
        public string LastIp { get; set; }
        public bool IsValid { get; set; }
        public virtual User User { get; set; }
    }
}
