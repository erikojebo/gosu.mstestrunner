using System;
using System.Linq;
using System.Reflection;
using Gosu.MsTestRunner.Core.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gosu.MsTestRunner.Core.Runner
{
    public class AssemblyRunner : MarshalByRefObject
    {
        public void RunTests(AssemblyConfiguration assemblyConfiguration)
        {
            
        }

        public void RunTests(string assemblyPath)
        {
            Assembly assembly;

            try
            {
                assembly = Assembly.LoadFrom(assemblyPath);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Failed to load assembly: {assemblyPath}");
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
                    .OrderBy(x => GetInheritanceHierarchyDepth(x.DeclaringType))
                    .ToList();

                var className = testClass.Name;

                var testCleanupMethods =
                    testClass.GetMethods().Where(x => x.GetCustomAttribute<TestCleanupAttribute>() != null)
                    .OrderBy(x => GetInheritanceHierarchyDepth(x.DeclaringType))
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
                            expectedExceptionAttribute.AllowDerivedTypes &&
                            actualExceptionType.IsSubclassOf(expectedExceptionType);

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
        }

        private object GetInheritanceHierarchyDepth(Type arg, int startingDepth = 0)
        {
            if (arg.BaseType == null)
            {
                return startingDepth;
            }

            return GetInheritanceHierarchyDepth(arg.BaseType, startingDepth + 1);
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