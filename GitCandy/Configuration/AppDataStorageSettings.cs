using Microsoft.Extensions.FileProviders;

namespace GitCandy.Configuration
{
    public class AppDataStorageSettings
    {
        public PhysicalFileProvider FileProvider { get; set; }
    }
}
