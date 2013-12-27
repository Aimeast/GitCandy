using System;

namespace GitCandy.Configuration
{
    [AttributeUsage(AttributeTargets.Property)]
    public class RecommendedValueAttribute : Attribute
    {
        public RecommendedValueAttribute(object value)
        {
            Value = value;
        }

        public object Value { get; private set; }
    }
}