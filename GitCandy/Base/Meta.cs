using System.IO;
using System.Linq;
using System.Reflection;

namespace GitCandy.Base
{
    public static class Meta
    {
        static readonly object SyncRoot = new object();

        static string _buildingInformation;
        public static string BuildingInformation
        {
            get
            {
                if (_buildingInformation == null)
                    lock (SyncRoot)
                        if (_buildingInformation == null)
                            try
                            {
                                var assembly = Assembly.GetExecutingAssembly();
                                var name = assembly.GetManifestResourceNames().Single(s => s.EndsWith(".Information"));
                                using (var stream = assembly.GetManifestResourceStream(name))
                                using (var reader = new StreamReader(stream))
                                {
                                    _buildingInformation = reader.ReadToEnd();
                                }
                            }
                            catch
                            {
                                _buildingInformation = "";
                            }

                return _buildingInformation;
            }
        }
    }
}