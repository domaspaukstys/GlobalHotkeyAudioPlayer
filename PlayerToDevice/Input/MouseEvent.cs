using System;

namespace PlayerToDevice.Input
{
    [Flags]
    internal enum MouseEvent : uint
    {
        Absolute = 0x8000,
        Hwheel = 0x01000,
        Move = 0x0001,
        MoveNocoalesce = 0x2000,
        Leftdown = 0x0002,
        Leftup = 0x0004,
        Rightdown = 0x0008,
        Rightup = 0x0010,
        Middledown = 0x0020,
        Middleup = 0x0040,
        Virtualdesk = 0x4000,
        Wheel = 0x0800,
        Xdown = 0x0080,
        Xup = 0x0100
    }
}