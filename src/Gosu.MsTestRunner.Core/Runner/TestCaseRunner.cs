using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gosu.MsTestRunner.Core.Runner
{
    public class TestCaseRunner : MarshalByRefObject
    {
        public event Action<TestCase> TestCaseStarting = x => {};
        public event Action<TestCase, TestResult> TestCaseFinished = (x, y) => {};

        public async Task Run(IEnumerable<TestCase> testCases)
        {
            foreach (var testCase in testCases)
            {
                await Task.Run(() =>
                {
                    TestCaseStarting(testCase);

                    var result = testCase.Run();

                    TestCaseFinished(testCase, result);
                });
            }
        }

        public TestResult Run(TestCase testCase)
         {
             return new TestResult
             {
                 WasIgnored = true
             };
         }
    }
}