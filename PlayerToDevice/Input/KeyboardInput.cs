using System;
using System.Runtime.InteropServices;

namespace PlayerToDevice.Input
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct KeyboardInput
    {
        internal VirtualKeyShort wVk;
        internal ScanCodeShort wScan;
        internal KeyEvent dwFlags;
        internal int time;
        internal UIntPtr dwExtraInfo;
    }
}