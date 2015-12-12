using System;
using System.ComponentModel;

namespace Gosu.MsTestRunner.Core.Runner
{
    [Serializable]
    public struct TestResult
    {
        public bool? WasSuccessful { get; set; }
        public bool WasInitializeSuccessful { get; set; }
        public bool? WasCleanUpSuccessful { get; set; }
        public bool WasIgnored { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public string Message { get; set; }
        public string InitializeMessage { get; set; }
        public string CleanUpMessage { get; set; }
    }
}