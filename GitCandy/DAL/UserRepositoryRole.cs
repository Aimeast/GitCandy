using System;
using System.Collections.Generic;

namespace GitCandy.DAL
{
    public partial class UserRepositoryRole
    {
        public long UserID { get; set; }
        public long RepositoryID { get; set; }
        public bool AllowRead { get; set; }
        public bool AllowWrite { get; set; }
        public bool IsOwner { get; set; }
        public virtual Repository Repository { get; set; }
        public virtual User User { get; set; }
    }
}
