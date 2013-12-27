using System;
using System.Security.Cryptography;
using System.Text;

namespace GitCandy.Security
{
    [PasswordProviderVersion(2)]
    public sealed class PasswordProviderV2 : PasswordProvider
    {
        const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        private readonly Random _rnd = new Random();

        public override string Compute(long uid, string username, string password)
        {
            var md5 = new MD5CryptoServiceProvider();
            var data = Encoding.Unicode.GetBytes("GitCandy_" + uid + "#" + username + "@" + password);
            data = md5.ComputeHash(data);
            return BitConverter.ToString(data).Replace("-", "");
        }

        public override string GenerateRandomPassword(int length)
        {
            var result = string.Empty;
            for (int i = 0; i < length; i++)
                result += Chars[_rnd.Next(Chars.Length)];
            return result;
        }
    }
}