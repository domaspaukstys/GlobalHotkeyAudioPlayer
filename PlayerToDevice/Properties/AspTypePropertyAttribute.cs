using System;

namespace PlayerToDevice.Annotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class AspTypePropertyAttribute : Attribute
    {
        public AspTypePropertyAttribute(bool createConstructorReferences)
        {
            CreateConstructorReferences = createConstructorReferences;
        }

        public bool CreateConstructorReferences { get; private set; }
    }
}