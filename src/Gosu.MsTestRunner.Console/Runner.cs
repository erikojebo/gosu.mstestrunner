using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Gosu.MsTestRunner.Core.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

            try
            {
                var fileContents = File.ReadAllText(configPath);
                var config = JsonConvert.DeserializeObject<RunnerConfiguration>(fileContents);

                Run(config);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Could not parse configuration file.");
                System.Console.WriteLine("Exception:");
                System.Console.WriteLine(ex);
            }
        }

        public static void Run(RunnerConfiguration config)
        {
            foreach (var assemblyConfiguration in config.Assemblies)
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
                ApplicationBase = assemblyBaseDirectory
            };

            AppDomain testDomain = AppDomain.CreateDomain($"{assemblyFileName}Domain", null, appDomainSetup);

            testDomain.ExecuteAssembly(assemblyConfiguration.Path);

            Assembly assembly;

            try
            {
                assembly = Assembly.LoadFrom(assemblyConfiguration.Path);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Failed to load assembly: {assemblyConfiguration.Path}");
                System.Console.WriteLine("Exception:");
                System.Console.WriteLine(ex);

                return;
            }

            var testClasses = assembly.GetTypes().Where(x => x.GetCustomAttribute<TestClassAttribute>() != null);

            foreach (var testClass in testClasses)
            {
                var testMethods = testClass.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(x => x.GetCustomAttribute<TestMethodAttribute>() != null)
                    .Where(x => x.GetCustomAttribute<IgnoreAttribute>() == null);

                var testInitializeMethods =
                    testClass.GetMethods().Where(x => x.GetCustomAttribute<TestInitializeAttribute>() != null)
                    .ToList();

                var testCleanupMethods =
                    testClass.GetMethods().Where(x => x.GetCustomAttribute<TestCleanupAttribute>() != null)
                    .ToList();

                foreach (var testMethod in testMethods)
                {
                    var expectedExceptionAttribute = testMethod.GetCustomAttribute<ExpectedExceptionAttribute>();
                    var testClassInstance = Activator.CreateInstance(testClass);

                    foreach (var testInitializeMethod in testInitializeMethods)
                    {
                        testInitializeMethod.Invoke(testClassInstance, null);
                    }

                    try
                    {
                        testMethod.Invoke(testClassInstance, null);
                    }
                    catch (TargetInvocationException ex)
                    {
                        if (ex.InnerException == null || expectedExceptionAttribute == null)
                            LogTestFailureException(testMethod, ex);

                        var actualExceptionType = ex.InnerException.GetType();
                        var expectedExceptionType = expectedExceptionAttribute.ExceptionType;

                        var wasExceptionExpected =
                                 expectedExceptionType == actualExceptionType ||
                                 expectedExceptionAttribute.AllowDerivedTypes && actualExceptionType.IsSubclassOf(expectedExceptionType);

                        if (!wasExceptionExpected)
                        {
                            LogTestFailureException(testMethod, ex);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogTestFailureException(testMethod, ex);
                    }

                    System.Console.Write(".");
                    //System.Console.WriteLine($"{testMethod.Name}: Succeeded");

                    foreach (var testCleanupMethod in testCleanupMethods)
                    {
                        testCleanupMethod.Invoke(testClassInstance, null);
                    }
                }
            }

            AppDomain.Unload(testDomain);
        }

        private static void LogTestFailureException(MethodInfo testMethod, Exception ex)
        {
            System.Console.WriteLine();
            System.Console.WriteLine($"{testMethod.Name}: Failed");
            System.Console.WriteLine(ex.Message);
            System.Console.WriteLine("Exception:");
            System.Console.WriteLine(ex);
            System.Console.WriteLine();
        }
    }
}