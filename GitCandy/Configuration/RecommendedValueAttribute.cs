using System;

namespace GitCandy.Configuration
{
    [AttributeUsage(AttributeTargets.Property)]
    public class RecommendedValueAttribute : Attribute
    {
        public RecommendedValueAttribute(object recommendedValue, object defaultValue = null)
        {
            RecommendedValue = recommendedValue;
            DefaultValue = defaultValue;
        }

        public object RecommendedValue { get; private set; }
        public object DefaultValue { get; private set; }
    }
}