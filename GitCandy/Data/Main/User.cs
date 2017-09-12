using System;

namespace GitCandy.Data.Main
{
    public class User
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Nickname { get; set; }
        public string Email { get; set; }
        public int PasswordVersion { get; set; }
        public string Password { get; set; }
        public string Description { get; set; }
        public bool IsSystemAdministrator { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
