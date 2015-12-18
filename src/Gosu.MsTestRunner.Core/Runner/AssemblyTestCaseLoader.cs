using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gosu.MsTestRunner.Core.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gosu.MsTestRunner.Core.Runner
{
    public class AssemblyTestCaseLoader : MarshalByRefObject
    {
        public IEnumerable<TestCase> Load(string assemblyPath)
        {
            Assembly assembly;

            try
            {
                assembly = Assembly.LoadFrom(assemblyPath);
            }
            catch (Exception ex)
            {
                //PublishEvent(x => x.OnAssemblyLoadFailed(assemblyPath, ex));

                return new List<TestCase>();
            }

            //PublishEvent(x => x.OnAssemblyTestRunStarting(assembly));

            var testCases = new List<TestCase>();

            var testClasses = assembly.GetTypes().Where(x => x.GetCustomAttribute<TestClassAttribute>() != null);

            foreach (var testClass in testClasses)
            {
                var testMethods = testClass.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(x => x.GetCustomAttribute<TestMethodAttribute>() != null);

                var testInitializeMethods =
                    testClass.GetMethods().Where(x => x.GetCustomAttribute<TestInitializeAttribute>() != null)
                    .OrderBy(x => x.DeclaringType.GetInheritanceHierarchyDepth())
                    .ToList();

                var testCleanupMethods =
                    testClass.GetMethods().Where(x => x.GetCustomAttribute<TestCleanupAttribute>() != null)
                    .OrderBy(x => x.DeclaringType.GetInheritanceHierarchyDepth())
                    .ToList();

                var assemblyTestCases = testMethods.Select(x => TestCase.Create(x, testInitializeMethods, testCleanupMethods, AppDomain.CurrentDomain));

                testCases.AddRange(assemblyTestCases);
            }

            return testCases;
        }
    }
}