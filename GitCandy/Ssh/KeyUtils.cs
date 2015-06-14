using System;
using System.Diagnostics.Contracts;
using System.Security.Cryptography;

namespace GitCandy.Ssh
{
    public static class KeyUtils
    {
        public static string GetFingerprint(string sshkey)
        {
            Contract.Requires(sshkey != null);

            using (var md5 = new MD5CryptoServiceProvider())
            {
                var bytes = Convert.FromBase64String(sshkey);
                bytes = md5.ComputeHash(bytes);
                return BitConverter.ToString(bytes).Replace('-', ':');
            }
        }

        private static AsymmetricAlgorithm GetAsymmetricAlgorithm(string type)
        {
            Contract.Requires(type != null);

            switch (type)
            {
                case "ssh-rsa":
                    return new RSACryptoServiceProvider();
                case "ssh-dss":
                    return new DSACryptoServiceProvider();
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }

        public static string GeneratePrivateKey(string type)
        {
            Contract.Requires(type != null);

            var alg = GetAsymmetricAlgorithm(type);
            return alg.ToXmlString(true);
        }

        public static string[] SupportedAlgorithms
        {
            get { return new string[] { "ssh-rsa", "ssh-dss" }; }
        }
    }
}
