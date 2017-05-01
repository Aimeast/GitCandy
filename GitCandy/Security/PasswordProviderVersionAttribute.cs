using System;

namespace GitCandy.Security
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PasswordProviderVersionAttribute : Attribute
    {
        public PasswordProviderVersionAttribute(int version)
        {
            Version = version;
        }

        public int Version { get; private set; }
    }
}
