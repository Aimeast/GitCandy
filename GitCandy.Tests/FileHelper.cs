using Microsoft.Extensions.FileProviders;
using System.IO;

namespace GitCandy.Tests
{
    public static class FileHelper
    {
        public static IFileProvider GetFileProvider(string dir)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), dir);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var fileProvider = new PhysicalFileProvider(path);

            return fileProvider;
        }
    }
}
