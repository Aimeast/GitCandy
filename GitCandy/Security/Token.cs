using System;

namespace GitCandy.Security
{
    public class Token
    {
        private const double AuthPeriod = 21600; // 15 days
        private const double RenewalPeriod = 1440; // 1 day

        private Token() { }

        public Token(Guid authCode, long userID, string username, string nickname, bool isSystemAdministrator, DateTime? expires = null)
        {
            AuthCode = authCode;
            UserID = userID;
            Username = username;
            Nickname = nickname;
            IsSystemAdministrator = isSystemAdministrator;

            var now = DateTime.Now;
            IssueDate = now;
            Expires = expires ?? now.AddMinutes(AuthPeriod);
        }

        public Guid AuthCode { get; private set; }
        public long UserID { get; private set; }
        public string Username { get; private set; }
        public string Nickname { get; private set; }
        public bool IsSystemAdministrator { get; private set; }

        public DateTime Expires { get; private set; }
        public DateTime IssueDate { get; private set; }
        public string LastIp { get; set; }

        public bool Expired { get { return Expires > DateTime.Now.AddMinutes(AuthPeriod); } }

        public bool RenewIfNeed()
        {
            var now = DateTime.Now;
            if (Expires > now && (Expires - now).TotalMinutes < AuthPeriod - RenewalPeriod)
            {
                Expires = now.AddMinutes(AuthPeriod);
                return true;
            }
            return false;
        }

        public static DateTime AuthorizationExpires
        {
            get
            {
                return DateTime.Now.AddMinutes(AuthPeriod);
            }
        }
    }
}
