using System;
using System.Security.Cryptography;
using System.Text;

namespace GitCandy.Security
{
    [PasswordProviderVersion(1)]
    public sealed class PasswordProviderV1 : PasswordProvider
    {
        public override string Compute(long uid, string username, string password)
        {
            using (var md5 = MD5.Create())
            {
                var data = Encoding.UTF8.GetBytes(password);
                data = md5.ComputeHash(data);
                return BitConverter.ToString(data).Replace("-", "");
            }
        }
    }
}
