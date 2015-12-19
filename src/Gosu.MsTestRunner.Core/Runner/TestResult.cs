using System;
using System.ComponentModel;
using System.Text;

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
        public string TestExecutionMessage { get; set; }
        public string InitializeMessage { get; set; }
        public string CleanUpMessage { get; set; }

        public string CombinedMessage
        {
            get
            {
                var sb = new StringBuilder();

                if (!string.IsNullOrWhiteSpace(InitializeMessage))
                {
                    sb.AppendLine();
                    sb.AppendLine("TEST INITIALIZE ----------------------------------------");
                    sb.AppendLine(InitializeMessage);
                    sb.AppendLine();
                    sb.AppendLine();
                }

                if (!string.IsNullOrWhiteSpace(TestExecutionMessage))
                {
                    sb.AppendLine();
                    sb.AppendLine("TEST EXECUTION ----------------------------------------");
                    sb.AppendLine(TestExecutionMessage);
                    sb.AppendLine();
                    sb.AppendLine();
                }

                if (!string.IsNullOrWhiteSpace(CleanUpMessage))
                {
                    sb.AppendLine();
                    sb.AppendLine("TEST CLEANUP ----------------------------------------");
                    sb.AppendLine(CleanUpMessage);
                    sb.AppendLine();
                    sb.AppendLine();
                }

                return sb.ToString();
            }
        }
    }
}