using System;

namespace GitCandy.Configuration
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ConfigurationKeyAttribute : Attribute
    {
        public ConfigurationKeyAttribute(string key)
        {
            Key = key;
        }

        public string Key { get; private set; }
    }
}
