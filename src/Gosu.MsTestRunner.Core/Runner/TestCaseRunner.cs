using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gosu.MsTestRunner.Core.Runner
{
    public class TestCaseRunner : MarshalByRefObject
    {
        public event Action<TestCase> TestCaseStarting = x => {};
        public event Action<TestCase, TestResult> TestCaseFinished = (x, y) => {};

        public async Task Run(IEnumerable<TestCase> testCases, bool allowParallelism)
        {
            if (allowParallelism)
            {
                await RunInParallel(testCases);
            }
            else
            {
                await Run(testCases);
            }
        }

        private async Task RunInParallel(IEnumerable<TestCase> testCases)
        {
            await testCases.ForEachAsync(Run, 5);
        }

        private async Task Run(IEnumerable<TestCase> testCases)
        {
            foreach (var testCase in testCases)
            {
                await Task.Run(() =>
                {
                    Run(testCase);
                });
            }
        }

        private void Run(TestCase testCase)
        {
            TestCaseStarting(testCase);

            var result = testCase.Run();

            TestCaseFinished(testCase, result);
        }
    }
}