using System;
using System.Diagnostics;
using System.Windows.Input;

namespace Hook
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            bool success = HotKeysHook.Register(() => { Debug.Print("Works1"); }, Key.A, Key.LeftCtrl);
            Console.WriteLine(success);
            success = HotKeysHook.Register(() => { Debug.Print("Works2"); }, new Key[] { Key.LeftCtrl, Key.A });
            Console.WriteLine(success);
            Console.ReadKey();
        }
    }
}