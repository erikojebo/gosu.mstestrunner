using System;
using System.Reflection;
using Gosu.MsTestRunner.Core.Listeners.Events;

namespace Gosu.MsTestRunner.Core.Listeners
{
    public interface ITestRunEventListener
    {
        void OnAssemblyTestRunStarting(Assembly assembly);
        void OnAssemblyTestRunFinished();
        void OnTestFailure(TestFailure testFailure);
        void OnTestPass(TestPass testPass);
        void OnAssemblyLoadFailed(string assemblyPath, Exception exception);
    }
}