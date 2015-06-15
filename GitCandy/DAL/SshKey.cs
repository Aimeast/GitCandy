using System;
using System.Collections.Generic;

namespace GitCandy.DAL
{
    public partial class SshKey
    {
        public long ID { get; set; }
        public long UserID { get; set; }
        public string KeyType { get; set; }
        public string Fingerprint { get; set; }
        public string PublicKey { get; set; }
        public System.DateTime ImportData { get; set; }
        public System.DateTime LastUse { get; set; }
        public virtual User User { get; set; }
    }
}
