using System;
using System.IO;
using System.Linq;
using Gosu.MsTestRunner.Core.Config;
using Gosu.MsTestRunner.Core.Runner;
using Newtonsoft.Json;

namespace Gosu.MsTestRunner.Console
{
    public class Runner
    {
        public static void Run(string configPath)
        {
            if (!File.Exists(configPath))
            {
                System.Console.WriteLine("No configuration file could be found at the specified path");
                return;
            }

            RunnerConfiguration config;

            try
            {
                var fileContents = File.ReadAllText(configPath);
                config = JsonConvert.DeserializeObject<RunnerConfiguration>(fileContents);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Could not parse configuration file.");
                System.Console.WriteLine("Exception:");
                System.Console.WriteLine(ex);

                return;
            }

            Run(config);
        }

        public static void Run(RunnerConfiguration config)
        {
            foreach (var assemblyConfiguration in config.Assemblies.Where(x => x.Platform != "x86"))
            {
                Run(assemblyConfiguration);
            }
        }

        private static void Run(AssemblyConfiguration assemblyConfiguration)
        {
            var assemblyFileName = Path.GetFileNameWithoutExtension(assemblyConfiguration.Path);
            var assemblyBaseDirectory = Path.GetDirectoryName(assemblyConfiguration.Path);

            AppDomainSetup appDomainSetup = new AppDomainSetup
            {
                ApplicationBase = assemblyBaseDirectory,
                ConfigurationFile = assemblyConfiguration.Path + ".config"
            };

            AppDomain testDomain = AppDomain.CreateDomain($"{assemblyFileName}Domain", null, appDomainSetup);

            var runner = (AssemblyRunner)testDomain.CreateInstanceFromAndUnwrap(
                //typeof(AssemblyRunner).Assembly.FullName, 
                typeof(AssemblyRunner).Assembly.Location,
                typeof(AssemblyRunner).FullName);

            try
            {
                runner.RunTests(assemblyConfiguration.Path);
            }
            catch (Exception)
            {
            }
            finally
            {
                AppDomain.Unload(testDomain);
            }
        }
    }
}