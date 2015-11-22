using System;
using System.IO;
using Gosu.MsTestRunner.Core.Config;
using Newtonsoft.Json;

namespace Gosu.MsTestRunner.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Communication with x64 specific process: IpcChannel

            /*
            {
                "assemblies": [
                    { "path" : "c:\foo\bar.dll", "allowParallel": true },
                    { "path" : "c:\bar\baz.dll", "allowParallel": false }
                ]
            }
            */

            if (args.Length == 0)
                System.Console.WriteLine("Usage: gosu.mstestrunner.console <path-to-config-file>");

            var configPath = args[0];

            Runner.Run(configPath);

#if DEBUG
            System.Console.WriteLine();
            System.Console.WriteLine("Press any key to exit...");
            System.Console.Read();
#endif
        }
    }
}