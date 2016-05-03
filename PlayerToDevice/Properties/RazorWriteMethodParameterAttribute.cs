using System;

namespace PlayerToDevice.Annotations
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class RazorWriteMethodParameterAttribute : Attribute
    {
    }
}