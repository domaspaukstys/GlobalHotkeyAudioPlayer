using System;

namespace PlayerToDevice.Annotations
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class RazorInjectionAttribute : Attribute
    {
        public RazorInjectionAttribute([NotNull] string type, [NotNull] string fieldName)
        {
            Type = type;
            FieldName = fieldName;
        }

        [NotNull]
        public string Type { get; private set; }

        [NotNull]
        public string FieldName { get; private set; }
    }
}