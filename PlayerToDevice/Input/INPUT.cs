using System.Runtime.InteropServices;

namespace PlayerToDevice.Input
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Input
    {
        internal InputType type;
        internal InputUnion U;

        internal static int Size
        {
            get { return Marshal.SizeOf(typeof(Input)); }
        }
    }
}