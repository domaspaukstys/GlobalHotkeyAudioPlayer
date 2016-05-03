using System;
using System.Runtime.InteropServices;

namespace TestHook
{
    public class KeyboardHook
    {

        public delegate int KeyboardProcess(int code, int wParam, ref KeyboardParams lParam);

        const int WinHookKeyboard = 13;
        const int WinMesKeyDown = 0x100;
        const int WinMesKeyUp = 0x101;
        const int WinMesSysKeyDown = 0x104;
        const int WinMesSysKeyUp = 0x105;

        // ReSharper disable InconsistentNaming
        public struct KeyboardParams
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        private IntPtr _hookHandle = IntPtr.Zero;

        // ReSharper restore InconsistentNaming

        public void Hook()
        {
            IntPtr instance = LoadLibrary("User32");
            _hookHandle = SetWindowsHookEx(WinHookKeyboard, ProcessHook, instance, 0);
        }

        public void Unhook()
        {
            UnhookWindowsHookEx(_hookHandle);
        }

        public int ProcessHook(int code, int wParam, ref KeyboardParams lParam)
        {
            if (code >= 0)
            {
                int key = lParam.vkCode;
                if ((wParam == WinMesKeyDown || wParam == WinMesSysKeyDown))
                {
                }
                else if ((wParam == WinMesKeyUp || wParam == WinMesSysKeyUp))
                {
                }
            }

            return CallNextHookEx(_hookHandle, code, wParam, ref lParam);
        }

        /// <summary>
        /// Sets the windows hook, do the desired event, one of hInstance or threadId must be non-null
        /// </summary>
        /// <param name="idHook">The id of the event you want to hook</param>
        /// <param name="callback">The callback.</param>
        /// <param name="instance">The handle you want to attach the event to, can be null</param>
        /// <param name="threadId">The thread you want to attach the event to, can be null</param>
        /// <returns>a handle to the desired hook</returns>
        [DllImport("user32.dll")]
        static extern IntPtr SetWindowsHookEx(int idHook, KeyboardProcess callback, IntPtr instance, uint threadId);

        /// <summary>
        /// Unhooks the windows hook.
        /// </summary>
        /// <param name="instance">The hook handle that was returned from SetWindowsHookEx</param>
        /// <returns>True if successful, false otherwise</returns>
        [DllImport("user32.dll")]
        static extern bool UnhookWindowsHookEx(IntPtr instance);

        /// <summary>
        /// Calls the next hook.
        /// </summary>
        /// <param name="idHook">The hook id</param>
        /// <param name="nCode">The hook code</param>
        /// <param name="wParam">The wparam.</param>
        /// <param name="lParam">The lparam.</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        static extern int CallNextHookEx(IntPtr idHook, int nCode, int wParam, ref KeyboardParams lParam);

        /// <summary>
        /// Loads the library.
        /// </summary>
        /// <param name="lpFileName">Name of the library</param>
        /// <returns>A handle to the library</returns>
        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string lpFileName);
    }
}
