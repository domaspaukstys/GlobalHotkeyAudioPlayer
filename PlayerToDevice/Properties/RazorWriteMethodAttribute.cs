using System;

namespace PlayerToDevice.Annotations
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RazorWriteMethodAttribute : Attribute
    {
    }
}