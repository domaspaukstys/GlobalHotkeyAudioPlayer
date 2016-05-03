using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestHook
{
    class Program
    {
        static void Main(string[] args)
        {
            KeyboardHook hook = new KeyboardHook();
            hook.Hook();

            while (Console.ReadKey().Key != ConsoleKey.Escape) { Thread.Sleep(1000); }
        }
    }
}
