using System;

namespace GitCandy.Configuration
{
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class RecommendedValueResloverAttribute : Attribute
    {
        public abstract object GetValue();
    }
}
