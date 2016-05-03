using System.Runtime.InteropServices;

namespace PlayerToDevice.Input
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct InputUnion
    {
        [FieldOffset(0)] internal MouseInput mi;
        [FieldOffset(0)] internal KeyboardInput ki;
        [FieldOffset(0)] internal HardwareInput hi;
    }
}