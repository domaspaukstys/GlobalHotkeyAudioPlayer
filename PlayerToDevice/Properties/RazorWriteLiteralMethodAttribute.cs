using System;

namespace PlayerToDevice.Annotations
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RazorWriteLiteralMethodAttribute : Attribute
    {
    }
}