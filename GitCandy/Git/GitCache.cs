using GitCandy.Configuration;
using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

namespace GitCandy.Git
{
    public static class GitCache
    {
        public static bool Enabled { get; private set; }
        private static string CachePath;

        public static void Initialize()
        {
            CachePath = UserConfiguration.Current.CachePath;
            DirectoryInfo info;
            try
            {
                info = new DirectoryInfo(CachePath);
                if (!info.Exists)
                    info.Create();
                Enabled = true;
            }
            catch
            {
                Enabled = false;
                return;
            }

            var path = typeof(GitCache).Assembly.Location;
            var md5 = new MD5CryptoServiceProvider();
            var data = md5.ComputeHash(File.ReadAllBytes(path));
            var hash = BitConverter.ToString(data).Replace("-", "");

            var filename = Path.Combine(CachePath, "version");
            if (!File.Exists(filename) || File.ReadAllText(filename) != hash)
            {
                foreach (var dir in info.GetDirectories())
                    dir.Delete(true);
                foreach (var file in info.GetFiles())
                    file.Delete();

                File.WriteAllText(filename, hash);
            }
        }

        public static void Set(string objectish, string key, byte[] data, bool overwritten = false)
        {
            if (!Enabled)
                return;

            Contract.Requires(objectish != null);
            Contract.Requires(key != null);
            Contract.Requires(data != null);

            var path = GetPath(objectish, key, true);
            lock (path)
                if (overwritten || !File.Exists(path))
                    File.WriteAllBytes(path, data);
        }

        public static void Set<T>(string objectish, string key, T obj, bool overwritten = false)
        {
            if (!Enabled)
                return;

            Contract.Requires(objectish != null);
            Contract.Requires(key != null);
            Contract.Requires(obj != null);

            var path = GetPath(objectish, key, true);
            lock (path)
                if (overwritten || !File.Exists(path))
                {
                    var xs = new BinaryFormatter();
                    using (var fs = File.Open(path, FileMode.Create))
                    {
                        xs.Serialize(fs, obj);
                        fs.Flush();
                    }
                }
        }

        public static byte[] Get(string objectish, string key)
        {
            if (!Enabled)
                return null;

            Contract.Requires(objectish != null);
            Contract.Requires(key != null);

            var path = GetPath(objectish, key, false);
            lock (path)
                if (!File.Exists(path))
                    return null;

            return File.ReadAllBytes(path);
        }

        public static T Get<T>(string objectish, string key)
        {
            if (!Enabled)
                return default(T);

            Contract.Requires(objectish != null);
            Contract.Requires(key != null);

            var path = GetPath(objectish, key, false);
            lock (path)
                if (!File.Exists(path))
                    return default(T);

            try
            {
                var xs = new BinaryFormatter();
                using (var fs = File.Open(path, FileMode.Open))
                {
                    var obj = xs.Deserialize(fs);
                    if (obj is T)
                        return (T)obj;
                    return default(T);
                }
            }
            catch
            {
                return default(T);
            }
        }

        public static bool Exists(string objectish, string key)
        {
            var path = GetPath(objectish, key, false);
            return File.Exists(path);
        }

        public static string GetCacheFilename(string objectish, string key, out bool exist, bool ensureDirectory = false)
        {
            var path = GetPath(objectish, key, ensureDirectory);
            exist = File.Exists(path);
            return path;
        }

        private static string GetPath(string objectish, string key, bool ensureDirectory)
        {
            var path = Path.Combine(CachePath, objectish.Substring(0, 2), objectish.Substring(2) + "-" + key);
            if (ensureDirectory)
            {
                var info = new FileInfo(path);
                if (!info.Directory.Exists)
                    info.Directory.Create();
            }
            return path;
        }
    }
}