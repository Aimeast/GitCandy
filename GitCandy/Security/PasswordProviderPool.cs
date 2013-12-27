using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace GitCandy.Security
{
    public static class PasswordProviderPool
    {
        private static readonly ProviderInfo[] Providers;
        private static readonly ReadOnlyDictionary<int, ConcurrentBag<PasswordProvider>> Pool;
        private delegate PasswordProvider PasswordProviderActivator();

        static PasswordProviderPool()
        {
            var types = new[] {
                typeof(PasswordProviderV1),
                typeof(PasswordProviderV2),
            };

            Providers = types
                .Select(s => new ProviderInfo
                {
                    Version = s.GetCustomAttributes(typeof(PasswordProviderVersionAttribute), false)
                        .Cast<PasswordProviderVersionAttribute>()
                        .Single()
                        .Version,
                    Type = s,
                    Creater = Expression.Lambda<PasswordProviderActivator>(Expression.New(s.GetConstructors().Single()))
                        .Compile(),
                })
                .OrderByDescending(s => s.Version)
                .ToArray();

            Pool = new ReadOnlyDictionary<int, ConcurrentBag<PasswordProvider>>(
                Providers
                    .ToDictionary(s => s.Version, s => new ConcurrentBag<PasswordProvider>()));

            LastVersion = Providers.Max(s => s.Version);
        }

        public static int LastVersion { get; private set; }

        public static PasswordProvider Take(int version = -1)
        {
            if (version < 1 || version > LastVersion)
                version = LastVersion;

            PasswordProvider provider;

            if (Pool[version].TryTake(out provider))
                return provider;

            var info = Providers.First(s => s.Version == version);

            return info.Creater();
        }

        public static void Revert(PasswordProvider provider)
        {
            if (provider != null)
                Pool[provider.Version].Add(provider);
        }

        private class ProviderInfo
        {
            public int Version;
            public Type Type;
            public PasswordProviderActivator Creater;
        }
    }
}