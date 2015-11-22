using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Gosu.MsTestRunner.Core.Config;
using Gosu.MsTestRunner.Core.Listeners;
using Gosu.MsTestRunner.Core.Listeners.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gosu.MsTestRunner.Core.Runner
{
    public class AssemblyRunner : MarshalByRefObject
    {
        private List<ITestRunEventListener> _listeners;

        public AssemblyRunner()
        {
            _listeners = new List<ITestRunEventListener>
            {
                new TestResultConsoleLogger(),
            };
        }

        public void RunTests(AssemblyConfiguration assemblyConfiguration)
        {
            RunTests(assemblyConfiguration.Path);
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
                PublishEvent(x => x.OnAssemblyLoadFailed(assemblyPath, ex));
                
                return;
            }

            PublishEvent(x => x.OnAssemblyTestRunStarting(assembly));

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

                var testCleanupMethods =
                    testClass.GetMethods().Where(x => x.GetCustomAttribute<TestCleanupAttribute>() != null)
                    .OrderBy(x => GetInheritanceHierarchyDepth(x.DeclaringType))
                    .ToList();

                foreach (var testMethod in testMethods)
                {
                    RunTest(testMethod, testClass, testInitializeMethods, testCleanupMethods);
                }
            }

            PublishEvent(x => x.OnAssemblyTestRunFinished());
        }

        private void RunTest(MethodInfo testMethod, Type testClass, List<MethodInfo> testInitializeMethods, List<MethodInfo> testCleanupMethods)
        {
            var testClassInstance = Activator.CreateInstance(testClass);

            foreach (var testInitializeMethod in testInitializeMethods)
            {
                testInitializeMethod.Invoke(testClassInstance, null);
            }

            try
            {
                testMethod.Invoke(testClassInstance, null);
                PublishTestPassedEvent(testMethod);
            }
            catch (TargetInvocationException actualException)
            {
                if (WasExceptionExpected(testMethod, actualException))
                {
                    PublishTestPassedEvent(testMethod);
                }
                else
                {
                    PublishTestFailureEvent(testMethod, actualException);
                }
            }
            catch (Exception ex)
            {
                PublishTestFailureEvent(testMethod, ex);
            }

            foreach (var testCleanupMethod in testCleanupMethods)
            {
                testCleanupMethod.Invoke(testClassInstance, null);
            }
        }

        private static bool WasExceptionExpected(MethodInfo testMethod, TargetInvocationException actualException)
        {
            var expectedExceptionAttribute = testMethod.GetCustomAttribute<ExpectedExceptionAttribute>();

            if (actualException.InnerException == null || expectedExceptionAttribute == null)
                return false;

            var actualExceptionType = actualException.InnerException.GetType();
            var expectedExceptionType = expectedExceptionAttribute.ExceptionType;

            var wasExceptionExpected =
                expectedExceptionType == actualExceptionType ||
                expectedExceptionAttribute.AllowDerivedTypes &&
                actualExceptionType.IsSubclassOf(expectedExceptionType);

            return wasExceptionExpected;
        }

        private void PublishTestPassedEvent(MethodInfo testMethod)
        {
            PublishEvent(x => x.OnTestPass(new TestPass(testMethod)));
        }

        private object GetInheritanceHierarchyDepth(Type arg, int startingDepth = 0)
        {
            if (arg.BaseType == null)
            {
                return startingDepth;
            }

            return GetInheritanceHierarchyDepth(arg.BaseType, startingDepth + 1);
        }

        private void PublishTestFailureEvent(MethodInfo testMethod, Exception ex)
        {
            PublishEvent(x => x.OnTestFailure(new TestFailure(testMethod, ex)));
        }

        private void PublishEvent(Action<ITestRunEventListener> eventPublishAction)
        {
            foreach (var eventListener in _listeners)
            {
                eventPublishAction(eventListener);
            }
        }
    }
}