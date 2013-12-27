using System;
using System.Security.Cryptography;
using System.Text;

namespace GitCandy.Security
{
    [PasswordProviderVersion(1)]
    public sealed class PasswordProviderV1 : PasswordProvider
    {
        private readonly Random _rnd = new Random();

        public override string Compute(long uid, string username, string password)
        {
            var md5 = new MD5CryptoServiceProvider();
            var data = Encoding.UTF8.GetBytes(password);
            data = md5.ComputeHash(data);
            return BitConverter.ToString(data).Replace("-", "");
        }

        public override string GenerateRandomPassword(int length)
        {
            var result = string.Empty;
            for (int i = 0; i < length; i++)
                result += _rnd.Next(10);
            return result;
        }
    }
}