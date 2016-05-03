using System;

namespace PlayerToDevice.Annotations
{
    /// <summary>
    ///     Indicates that IEnumerable, passed as parameter, is not enumerated.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class NoEnumerationAttribute : Attribute
    {
    }
}