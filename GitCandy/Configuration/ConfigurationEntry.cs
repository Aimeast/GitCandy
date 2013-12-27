using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace GitCandy.Configuration
{
    public abstract class ConfigurationEntry<TEntry> where TEntry : ConfigurationEntry<TEntry>, new()
    {
        private static TEntry _current;
        private static readonly object _sync = new object();
        private static readonly string _configurationPath = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["UserConfiguration"]);

        public static TEntry Current { get { return _current ?? Load(); } }

        [XmlIgnore]
        public bool IsNew { get; private set; }

        private static TEntry Load()
        {
            if (_current == null)
                lock (_sync)
                {
                    if (_current == null)
                        try
                        {
                            var serializer = new XmlSerializer(typeof(TEntry));
                            using (var stream = File.Open(_configurationPath, FileMode.Open))
                                _current = serializer.Deserialize(stream) as TEntry;
                        }
                        catch { }

                    if (_current == null)
                    {
                        _current = NewDefault();
                        _current.Save();
                    }
                }

            return _current;
        }

        public static TEntry NewDefault()
        {
            var entry = new TEntry();

            var properties = typeof(TEntry).GetProperties();
            foreach (var property in properties)
            {
                var attr = property.GetCustomAttributes(typeof(RecommendedValueAttribute), false).FirstOrDefault() as RecommendedValueAttribute;
                if (attr != null)
                    property.SetValue(entry, attr.Value);
            }
            entry.IsNew = true;

            return entry;
        }

        public void Save()
        {
            lock (_sync)
            {
                var serializer = new XmlSerializer(typeof(TEntry));

                var fileInfo = new FileInfo(_configurationPath);
                if (!fileInfo.Directory.Exists)
                    fileInfo.Directory.Create();

                using (var stream = fileInfo.Open(FileMode.Create))
                    serializer.Serialize(stream, this);

                _current = (TEntry)this;
                IsNew = false;
            }
        }
    }
}