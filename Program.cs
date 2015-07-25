using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace RPGNet
{
    class Program
    {
        static void Main(string[] args)
        {
            String CodeFile = String.Join(" ", args);
            if (CodeFile == "") return;
            String Code = Interpreter.getContent(CodeFile);
            String Name = new FileInfo(CodeFile).Name; Name = Name.Substring(0, Name.LastIndexOf('.'));
            String Compiler = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\ilasm.exe";
            
            Module.Run(Name, Code);
            foreach (String Line in Module.getCode())
            {
                Console.WriteLine(Line);
            }
            File.WriteAllLines(Name + ".il", Module.getCode());
            Process.Start(Compiler, '"' + Environment.CurrentDirectory + "\\" + Name + '"');
            Console.ReadLine();
        }
    }
}
