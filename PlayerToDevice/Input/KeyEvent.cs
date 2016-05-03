using System;

namespace PlayerToDevice.Input
{
    [Flags]
    internal enum KeyEvent : uint
    {
        Extendedkey = 0x0001,
        Keyup = 0x0002,
        Scancode = 0x0008,
        Unicode = 0x0004
    }
}