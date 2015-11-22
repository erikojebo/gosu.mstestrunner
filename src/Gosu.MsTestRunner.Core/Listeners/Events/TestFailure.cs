using System;
using System.Reflection;

namespace Gosu.MsTestRunner.Core.Listeners.Events
{
    public class TestFailure : TestRunEvent
    {
        public TestFailure(MethodInfo testMethod, Exception exception) : base(testMethod)
        {
            Exception = exception;
        }

        public readonly Exception Exception;
    }
}