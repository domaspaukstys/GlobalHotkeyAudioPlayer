using System;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace Hook
{
    public class KeyboardHook
    {
        private delegate int KeyboardProcess(int code, int wParam, ref KeyboardParams lParam);

        private const int WinHookKeyboard = 13;
        private const int WinMesKeyDown = 0x100;
        private const int WinMesKeyUp = 0x101;
        private const int WinMesSysKeyDown = 0x104;
        private const int WinMesSysKeyUp = 0x105;

        private static KeyboardHook _instance;
        protected static KeyboardHook Instance => _instance = _instance ?? new KeyboardHook();

        private KeyboardHook()
        {
            _library = LoadLibrary("User32");
            _process = ProcessHook;
        }

        ~KeyboardHook()
        {
            Unhook();
        }

        private readonly IntPtr _library;
        private int _handlers;
        private bool _hooked;
        private event EventHandler<KeyStateArgs> _keyStateChanged;
        public static event EventHandler<KeyStateArgs> KeyStateChanged
        {
            add
            {
                Instance._keyStateChanged += value;
                Instance.ChangeHook(true);
            }
            remove
            {
                Instance._keyStateChanged -= value;
                Instance.ChangeHook(false);
            }
        }

        private void ChangeHook(bool increase)
        {
            _handlers = increase ? _handlers + 1 : _handlers - 1;
            if (_handlers > 0)
            {
                Hook();
            }
            else
            {
                _handlers = 0;
                Unhook();
            }
        }

        private void Hook()
        {
            if (!_hooked)
            {
                _hookHandle = SetWindowsHookEx(WinHookKeyboard, _process, _library, 0);
                _hooked = true;
            }
        }

        private void Unhook()
        {
            if (_hooked)
            {
                UnhookWindowsHookEx(_hookHandle);
                _hooked = false;
            }
        }

        private int ProcessHook(int code, int wParam, ref KeyboardParams lParam)
        {
            if (code >= 0)
            {
                var key = KeyInterop.KeyFromVirtualKey(lParam.vkCode);

                switch (wParam)
                {
                    case WinMesKeyDown:
                    case WinMesSysKeyDown:
                        OnKeyStateChanged(new KeyStateArgs(key, false));
                        break;
                    case WinMesKeyUp:
                    case WinMesSysKeyUp:
                        OnKeyStateChanged(new KeyStateArgs(key, true));
                        break;
                }
            }

            return CallNextHookEx(_hookHandle, code, wParam, ref lParam);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, KeyboardProcess callback, IntPtr instance,
            uint threadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr instance);

        [DllImport("user32.dll")]
        private static extern int CallNextHookEx(IntPtr idHook, int nCode, int wParam, ref KeyboardParams lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string lpFileName);

        public class KeyStateArgs : EventArgs
        {
            public KeyStateArgs(Key key, bool pressed)
            {
                Key = key;
                Pressed = pressed;
            }

            public Key Key { get; }
            public bool Pressed { get; }
        }

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
        private KeyboardProcess _process;

        protected virtual void OnKeyStateChanged(KeyStateArgs e)
        {
            _keyStateChanged?.Invoke(this, e);
        }
    }
}