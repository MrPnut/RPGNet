using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGNet
{
    class Errors
    {
        public static void throwError(String Message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(Message);
            Console.WriteLine("Compiling terminated.");
            Console.ReadLine();
            Environment.Exit(1);
        }
        public static void throwNotice(String Message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(Message);
        }
    }
}
