using System;

namespace Gosu.MsTestRunner.Core.Exceptions
{
    public class GosuMsTestRunnerException : Exception
    {
        public GosuMsTestRunnerException()
        {
        }

        public GosuMsTestRunnerException(string message) : base(message)
        {
        }

        public GosuMsTestRunnerException(string message, params object[] formatParams) : base(string.Format(message, formatParams))
        {
        }

        public GosuMsTestRunnerException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}