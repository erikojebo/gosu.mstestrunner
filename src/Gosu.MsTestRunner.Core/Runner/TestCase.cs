using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gosu.MsTestRunner.Core.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gosu.MsTestRunner.Core.Runner
{
    public class TestCase : MarshalByRefObject
    {
        private readonly MethodInfo _testMethodInfo;
        private readonly IEnumerable<MethodInfo> _testInitializeMethodInfos;
        private readonly IEnumerable<MethodInfo> _testCleanUpMethodInfos;

        private TestCase(
            MethodInfo testMethodInfo,
            IEnumerable<MethodInfo> testInitializeMethodInfos,
            IEnumerable<MethodInfo> testCleanUpMethodInfos,
            AppDomain appDomain)
        {
            _testMethodInfo = testMethodInfo;

            _testInitializeMethodInfos = testInitializeMethodInfos
                .OrderBy(x => x.DeclaringType.GetInheritanceHierarchyDepth());

            _testCleanUpMethodInfos = testCleanUpMethodInfos
                .OrderBy(x => x.DeclaringType.GetInheritanceHierarchyDepth());

            Id = Guid.NewGuid();
            AppDomain = appDomain;
            Categories = new List<string>();
            DisplayName = testMethodInfo.Name.Replace("_", " ");
            AssemblyName = testMethodInfo.DeclaringType?.Assembly.GetName().Name;
            TestClassName = testMethodInfo.DeclaringType?.Name;
            IsIgnored = testMethodInfo.GetCustomAttribute<IgnoreAttribute>() != null;
        }

        public Guid Id { get; }
        public IEnumerable<string> Categories { get; }
        public string DisplayName { get; }
        public string TestClassName { get; }
        public string AssemblyName { get; }
        public bool IsIgnored { get; set; }
        public AppDomain AppDomain { get; }

        public static TestCase Create(
            MethodInfo testMethodInfo, 
            IEnumerable<MethodInfo> testInitializeMethodInfos, 
            IEnumerable<MethodInfo> testCleanUpMethodInfos, 
            AppDomain appDomain)
        {
            return new TestCase(testMethodInfo, testInitializeMethodInfos, testCleanUpMethodInfos, appDomain);
        }

        public TestResult Run()
        {
            if (IsIgnored)
            {
                return new TestResult
                {
                    WasIgnored = true,
                    ExecutionTime = TimeSpan.Zero
                };
            }

            var startTime = DateTime.Now;

            var testClassInstance = AppDomain.CreateInstance(_testMethodInfo.DeclaringType);

            try
            {
                foreach (var testInitializeMethodInfo in _testInitializeMethodInfos)
                {
                    testInitializeMethodInfo.Invoke(testClassInstance, null);
                }
            }
            catch (Exception ex)
            {
                return new TestResult
                {
                    WasInitializeSuccessful = false,
                    ExecutionTime = DateTime.Now - startTime,
                    InitializeMessage = $"Test initalization failed. Exception: {ex}"
                };
            }

            var testResult = new TestResult();
            
            try
            {
                _testMethodInfo.Invoke(testClassInstance, null);

                testResult.WasSuccessful = true;
            }
            catch (TargetInvocationException actualException)
            {
                if (WasExceptionExpected(actualException))
                {
                    testResult.WasSuccessful = true;
                }
                else
                {
                    testResult.WasSuccessful = false;
                    testResult.TestExecutionMessage = $"Unexpected exception thrown. Exception: {actualException}";
                }
            }
            catch (Exception ex)
            {
                testResult.TestExecutionMessage = $"Test failed. Exception: {ex}";
                testResult.WasSuccessful = false;
            }

            try
            {
                foreach (var testInitializeMethodInfo in _testCleanUpMethodInfos)
                {
                    testInitializeMethodInfo.Invoke(testClassInstance, null);
                }

                testResult.WasCleanUpSuccessful = true;
            }
            catch (Exception ex)
            {
                testResult.WasCleanUpSuccessful = false;
                testResult.CleanUpMessage = $"Test cleanup failed. Exception: {ex}";
            }

            testResult.ExecutionTime = DateTime.Now - startTime;

            return testResult;
        }

        private bool WasExceptionExpected(TargetInvocationException actualException)
        {
            var expectedExceptionAttribute = _testMethodInfo.GetCustomAttribute<ExpectedExceptionAttribute>();

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

    }
}