using System;
using System.Linq;

namespace GitCandy.Security
{
    public abstract class PasswordProvider : IDisposable
    {
        private int _version = -1;

        public virtual int Version
        {
            get
            {
                if (_version == -1)
                    _version = this.GetType()
                        .GetCustomAttributes(typeof(PasswordProviderVersionAttribute), false)
                        .Cast<PasswordProviderVersionAttribute>()
                        .Single()
                        .Version;
                return _version;
            }
        }
        public abstract string Compute(long uid, string username, string password);
        public abstract string GenerateRandomPassword(int length = 6);

        public void Dispose()
        {
            PasswordProviderPool.Revert(this);
        }
    }
}