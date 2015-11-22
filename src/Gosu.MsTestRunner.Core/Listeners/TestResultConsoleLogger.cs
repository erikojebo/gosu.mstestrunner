using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Gosu.MsTestRunner.Core.Listeners.Events;

namespace Gosu.MsTestRunner.Core.Listeners
{
    public class TestResultConsoleLogger : ITestRunEventListener
    {
        private readonly IList<TestFailure> _failedTestMethods = new List<TestFailure>();
        private readonly IList<TestPass> _passedTestMethods = new List<TestPass>();
        private DateTime _startTime;
        private DateTime _endTime;

        private TimeSpan ElapsedTime => _endTime - _startTime;

        public void OnAssemblyTestRunStarting(Assembly assembly)
        {
            _startTime = DateTime.Now;

            Console.WriteLine($"Starting test run for assembly {Path.GetFileNameWithoutExtension(assembly.Location)}...");
        }

        public void OnAssemblyTestRunFinished()
        {
            _endTime = DateTime.Now;

            Console.WriteLine();
            Console.WriteLine($"Test run finished. Elapsed time: {ElapsedTime.ToString("c")}");
            Console.WriteLine($"Passed: {_passedTestMethods.Count}, Failed: {_failedTestMethods.Count}");
            Console.WriteLine();
        }

        public void OnTestFailure(TestFailure testFailure)
        {
            _failedTestMethods.Add(testFailure);

            Console.WriteLine();
            Console.WriteLine("Test failed: " + testFailure.TestMethodName);
            Console.WriteLine("Exception:");
            Console.WriteLine(testFailure.Exception);
            Console.WriteLine();
        }

        public void OnTestPass(TestPass testPass)
        {
            _passedTestMethods.Add(testPass);

            Console.Write(".");
        }

        public void OnAssemblyLoadFailed(string assemblyPath, Exception exception)
        {
            Console.WriteLine();
            Console.WriteLine($"Failed to load assembly: {assemblyPath}");
            Console.WriteLine("Exception:");
            Console.WriteLine(exception);
            Console.WriteLine();
        }
    }
}