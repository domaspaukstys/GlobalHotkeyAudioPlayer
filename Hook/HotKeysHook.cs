using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Hook
{
    public class HotKeysHook
    {
        private static HotKeysHook _instance;
        private static HotKeysHook Instance => _instance = _instance ?? new HotKeysHook();

        private readonly HashSet<HotKey> _hotKeys;
        private readonly HashSet<Key> _pressedKeys;

        private HotKeysHook()
        {
            _hotKeys = new HashSet<HotKey>();
            _pressedKeys = new HashSet<Key>();
            KeyboardHook.KeyStateChanged += KeyStateChanged;
        }

        private void KeyStateChanged(object sender, KeyboardHook.KeyStateArgs args)
        {
            if (args.Pressed)
            {
                _pressedKeys.Add(args.Key);
                IEnumerable<HotKey> keys = _hotKeys.Where(x => x.Equals(_pressedKeys));
                foreach (HotKey key in keys)
                {
                    key.Invoke();
                }
            }
            else
            {
                _pressedKeys.Remove(args.Key);
            }
        }

        public static bool Register(Action action, params Key[] keys)
        {
            HotKey key = new HotKey(action, keys);
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
                foreach (Key key in keys)
                {
                    _keys.Add(key);
                }
            }

            private int Hash(IEnumerable<Key> keys)
            {
                unchecked
                {
                    int hash = 0;
                    foreach (Key key in _keys.OrderBy(x => x))
                    {
                        hash = (hash * 397) ^ key.GetHashCode();
                    }
                    return hash;
                }
            }

            public bool Equals(HotKey other)
            {
                return _keys.SetEquals(other._keys);
            }

            public bool Equals(Key[] other)
            {
                return GetHashCode().Equals(Hash(other));
            }

            public bool Equals(HashSet<Key> other)
            {
                return _keys.SetEquals(other);
            }

            public override bool Equals(object obj)
            {
                return obj is HotKey && Equals((HotKey)obj);
            }

            public override int GetHashCode()
            {
                return Hash(_keys);
            }

            public void Invoke()
            {
                _action?.BeginInvoke(null, this);
            }
        }
    }
}
