using System;
using System.Runtime.InteropServices;

namespace PlayerToDevice.Input
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct MouseInput
    {
        internal int dx;
        internal int dy;
        internal int mouseData;
        internal MouseEvent dwFlags;
        internal uint time;
        internal UIntPtr dwExtraInfo;
    }
}