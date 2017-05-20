using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using System.Linq;
using System.Reflection;

namespace GitCandy.Configuration
{
    public abstract class ConfigurationBase
    {
        public void AssignAsDefaultValue()
        {
            var properties = this.GetType().GetProperties();
            foreach (var property in properties)
            {
                var attr = property
                    .GetCustomAttributes<RecommendedValueAttribute>(false)
                    .FirstOrDefault();
                if (attr != null)
                {
                    property.SetValue(this, attr.DefaultValue ?? attr.RecommendedValue);
                    continue;
                }

                var reslover = property
                    .GetCustomAttributes<RecommendedValueResloverAttribute>(true)
                    .FirstOrDefault();
                if (reslover != null)
                {
                    property.SetValue(this, reslover.GetValue());
                    continue;
                }
            }
        }

        [JsonIgnore]
        public IFileInfo SettingsFileInfo { get; set; }
    }
}
