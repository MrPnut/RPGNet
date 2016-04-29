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
            Config Info = new Config("Config.cfg");

            Module.Run(Name, Code);
            File.WriteAllLines(Name + ".il", Module.getCode());

            String Compiler = Info.getConf("ilasm");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Process compiler = new Process();
            compiler.StartInfo.FileName = Compiler;
            compiler.StartInfo.Arguments = '"' + Environment.CurrentDirectory + "\\" + Name + '"';
            compiler.StartInfo.UseShellExecute = false;
            compiler.StartInfo.RedirectStandardOutput = true;
            compiler.Start();
            Console.WriteLine(compiler.StandardOutput.ReadToEnd());
            compiler.WaitForExit();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("End of compile.");
            Console.WriteLine("Exit code: " + compiler.ExitCode);

            Console.ReadLine();
        }
    }
}
