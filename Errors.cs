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
        public static void showInfo(String Message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(Message);
        }

        public static void showDefinedNotice(String ID)
        {
            switch (ID)
            {
                case "spacing":
                    Errors.showInfo("This can be caused by spaces within quotes. For example:");
                    Errors.showInfo("%Scan(' ':String) will crash within a program. Instead: ");
                    Errors.showInfo("%Scan('$s':String) will fix this problem.");
                    break;
            }
        }
    }
}
