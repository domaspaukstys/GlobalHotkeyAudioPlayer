using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace PlayerToDevice.Input
{
    public class HotKey : IDisposable
    {
        private readonly Key[] _keys;

        public HotKey(Key key, Action action, params Key[] modifiers)
        {
            Key = key;
            Action = action;
            _keys = Combine(key, modifiers);
            HotKeysHook.Register(action, _keys);
        }

        public Key Key { get; private set; }
        public Action Action { get; private set; }

        public void Dispose()
        {
            Unregister();
        }

        public void Unregister()
        {
            HotKeysHook.Unregister(_keys);
        }

        public static bool CanRegister(Key key, params Key[] modifiers)
        {
            return HotKeysHook.CanRegister(Combine(key, modifiers));
        }

        private static Key[] Combine(Key key, params Key[] modifiers)
        {
            var keys = new List<Key>();
            keys.Add(key);
            if (modifiers != null)
                keys.AddRange(modifiers);
            return keys.ToArray();
        }
    }
}