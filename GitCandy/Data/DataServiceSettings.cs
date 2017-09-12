using Microsoft.Extensions.FileProviders;

namespace GitCandy.Data
{
    public class DataServiceSettings
    {
        public IFileInfo MainDbFileInfo { get; set; }
        public IFileInfo CacheDbFileInfo { get; set; }
    }
}
