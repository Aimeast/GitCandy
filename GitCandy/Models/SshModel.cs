
namespace GitCandy.Models
{
    public class SshModel
    {
        public string Username { get; set; }
        public SshKey[] SshKeys { get; set; }

        public class SshKey
        {
            public string Name { get; set; }
        }
    }
}
