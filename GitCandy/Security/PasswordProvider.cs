using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;

namespace GitCandy.Security
{
    // Thread Safe
    public abstract class PasswordProvider
    {
        private const string PasswordChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        private static readonly ReadOnlyDictionary<int, PasswordProvider> _providers;
        private int _version = -1;

        static PasswordProvider()
        {
            var instaces = new PasswordProvider[] {
                new PasswordProviderV1(),
                new PasswordProviderV2(),
            };
            _providers = new ReadOnlyDictionary<int, PasswordProvider>(instaces.ToDictionary(s => s.Version, s => s));
            LastVersion = instaces.Max(s => s.Version);
        }

        public virtual int Version
        {
            get
            {
                if (_version == -1)
                    _version = this.GetType()
                        .GetTypeInfo()
                        .GetCustomAttributes(typeof(PasswordProviderVersionAttribute), false)
                        .Cast<PasswordProviderVersionAttribute>()
                        .First()
                        .Version;
                return _version;
            }
        }

        public static int LastVersion { get; private set; }

        public abstract string Compute(long uid, string username, string password);

        public virtual string GenerateRandomPassword(int length = 6)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var data = new byte[length];
                rng.GetBytes(data);

                var bigint = BigInteger.Abs(new Big​Integer(data));
                var charLength = PasswordChars.Length;
                var result = new char[length];
                for (var i = 0; i < length; i++)
                {
                    result[i] = PasswordChars[(int)(bigint % charLength)];
                    bigint = bigint / charLength;
                }

                return new string(result);
            }
        }

        public static PasswordProvider Peek(int version = -1)
        {
            if (version < 1 || version > LastVersion)
                version = LastVersion;

            return _providers[version];
        }
    }
}
