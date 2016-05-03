using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace PlayerToDevice.Input
{
    public class HotKeysHook
    {
        private static HotKeysHook _instance;

        private readonly HashSet<HotKey> _hotKeys;
        private readonly HashSet<Key> _pressedKeys;

        private HotKeysHook()
        {
            _hotKeys = new HashSet<HotKey>();
            _pressedKeys = new HashSet<Key>();
            KeyboardHook.KeyStateChanged += KeyStateChanged;
        }

        private static HotKeysHook Instance => _instance = _instance ?? new HotKeysHook();
        private bool hotkeyFired;
        private void KeyStateChanged(object sender, KeyboardHook.KeyStateArgs args)
        {
            if (args.Pressed)
            {
                hotkeyFired = false;
                _pressedKeys.Add(args.Key);
                var keys = _hotKeys.Where(x => x.Equals(_pressedKeys));
                foreach (var key in keys)
                {
                    args.Handled = true;
                    hotkeyFired = true;
                    key.Invoke();
                }
                Debug.Print("Down " + args.Key);
            }
            else
            {
                args.Handled = hotkeyFired;
                hotkeyFired = false;
                _pressedKeys.Remove(args.Key);
                Debug.Print("Up " + args.Key);
            }
        }

        public static bool Register(Action action, params Key[] keys)
        {
            var key = new HotKey(action, keys);
            return Instance._hotKeys.Add(key);
        }

        public static bool CanRegister(params Key[] keys)
        {
            return !Instance._hotKeys.Any(x => x.Equals(keys));
        }

        public static bool Unregister(params Key[] keys)
        {
            Instance._hotKeys.RemoveWhere(x => x.Equals(keys));
            return CanRegister(keys);
        }

        private class HotKey : IEquatable<HotKey>, IEquatable<Key[]>, IEquatable<HashSet<Key>>
        {
            private readonly Action _action;
            private readonly HashSet<Key> _keys;

            public HotKey(Action action, params Key[] keys)
            {
                _action = action;
                _keys = new HashSet<Key>();
                foreach (var key in keys)
                {
                    _keys.Add(key);
                }
            }

            public bool Equals(HashSet<Key> other)
            {
                return _keys.SetEquals(other);
            }

            public bool Equals(HotKey other)
            {
                return _keys.SetEquals(other._keys);
            }

            public bool Equals(Key[] other)
            {
                return GetHashCode().Equals(Hash(other));
            }

            private int Hash(IEnumerable<Key> keys)
            {
                unchecked
                {
                    var hash = 7;
                    foreach (var key in keys.OrderBy(x => x))
                    {
                        hash += hash*5 + key.GetHashCode();
                    }
                    return hash;
                }
            }

            public override bool Equals(object obj)
            {
                return obj is HotKey && Equals((HotKey) obj);
            }

            public override int GetHashCode()
            {
                return Hash(_keys);
            }

            public void Invoke()
            {
                Debug.Print("Invoke");
                _action?.BeginInvoke(null, null);
                Debug.Print("Invoke Finished");
            }
        }
    }
}