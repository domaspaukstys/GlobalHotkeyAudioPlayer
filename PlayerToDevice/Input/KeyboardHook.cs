using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace PlayerToDevice.Input
{
    public class KeyboardHook
    {
        private const int WinHookKeyboard = 13;
        private const int WinMesKeyDown = 0x100;
        private const int WinMesKeyUp = 0x101;
        private const int WinMesSysKeyDown = 0x104;
        private const int WinMesSysKeyUp = 0x105;

        private const uint MapvkVkToVsc = 0x00;
        private const uint MapvkVscToVk = 0x01;
        private const uint MapvkVkToChar = 0x02;
        private const uint MapvkVscToVkEx = 0x03;
        private const uint MapvkVkToVscEx = 0x04;

        private static KeyboardHook _instance;

        private readonly IntPtr _library;
        private int _handlers;
        private bool _hooked;
        private HashSet<VirtualKeyShort> _ignore;

        private IntPtr _hookHandle = IntPtr.Zero;
        private readonly KeyboardProcess _process;

        private KeyboardHook()
        {
            _ignore = new HashSet<VirtualKeyShort>();
            _library = LoadLibrary("User32");
            _process = ProcessHook;
        }

        protected static KeyboardHook Instance => _instance = _instance ?? new KeyboardHook();

        ~KeyboardHook()
        {
            Unhook();
        }

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
                if (!_ignore.Contains((VirtualKeyShort)lParam.VkCode))
                {
                    var key = KeyInterop.KeyFromVirtualKey(lParam.VkCode);
                    var scanCode = MapVirtualKey((uint)lParam.VkCode, MapvkVkToChar);
                    Debug.Print(scanCode.ToString());
                    bool handled = false;
                    switch (wParam)
                    {
                        case WinMesKeyDown:
                        case WinMesSysKeyDown:
                            KeyStateArgs downArgs = new KeyStateArgs(key, true);
                            OnKeyStateChanged(downArgs);
                            handled = downArgs.Handled;
                            break;
                        case WinMesKeyUp:
                        case WinMesSysKeyUp:
                            KeyStateArgs upArgs = new KeyStateArgs(key, false);
                            OnKeyStateChanged(upArgs);
                            handled = upArgs.Handled;
                            break;
                    }
                    if (handled)
                    {
                        return 1;
                    }
                }
            }

            return CallNextHookEx(_hookHandle, code, wParam, ref lParam);
        }

        public static void KeyDown(VirtualKeyShort k, bool ignoreRead = true)
        {
            if (ignoreRead)
            {
                Instance._ignore.Add(k);
            }
            SendInput(k, true);
        }

        private static void SendInput(VirtualKeyShort k, bool down)
        {
            Input input = new Input
            {
                U = new InputUnion(),
                type = InputType.Keyboard
            };
            //            int fromKey = KeyInterop.VirtualKeyFromKey(k);
            input.U.ki = new KeyboardInput
            {
                time = 0,
                wScan = (ScanCodeShort)MapVirtualKey((uint)k, MapvkVkToVsc)
            };
            //            input.U.ki.wVk = (VirtualKeyShort) fromKey;
            if (!down)
                input.U.ki.dwFlags = KeyEvent.Keyup;
            input.U.ki.dwFlags = input.U.ki.dwFlags | KeyEvent.Scancode;

            SendInput(1u, new[] { input }, Input.Size);
        }

        public static void KeyUp(VirtualKeyShort k, bool ignoreRead = true)
        {
            if (ignoreRead)
            {
                Instance._ignore.Remove(k);
            }
            SendInput(k, false);
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

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll")]
        internal static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] Input[] pInputs,
            int cbSize);

        protected virtual void OnKeyStateChanged(KeyStateArgs e)
        {
            _keyStateChanged?.Invoke(this, e);
        }

        private delegate int KeyboardProcess(int code, int wParam, ref KeyboardParams lParam);

        public class KeyStateArgs : EventArgs
        {
            public KeyStateArgs(Key key, bool pressed)
            {
                Key = key;
                Pressed = pressed;
            }

            public Key Key { get; }
            public bool Pressed { get; }
            public bool Handled { get; set; }
        }
    }

    // ReSharper disable InconsistentNaming
}