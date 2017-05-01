using System;
using System.Security.Cryptography;
using System.Text;

namespace GitCandy.Security
{
    [PasswordProviderVersion(2)]
    public sealed class PasswordProviderV2 : PasswordProvider
    {
        public override string Compute(long uid, string username, string password)
        {
            using (var md5 = MD5.Create())
            {
                var data = Encoding.Unicode.GetBytes("GitCandy_" + uid + "#" + username + "@" + password);
                data = md5.ComputeHash(data);
                return BitConverter.ToString(data).Replace("-", "");
            }
        }
    }
}
