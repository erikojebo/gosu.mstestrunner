using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gosu.MsTestRunner.Core.Runner
{
    public class TestSessionContext : IDisposable
    {
        public TestSessionContext()
        {
            TestCases = new List<TestCase>();
        }

        public IList<TestCase> TestCases { get; }

        public void AddTestCases(IEnumerable<TestCase> testCases)
        {
            foreach (var testCase in testCases)
            {
                TestCases.Add(testCase);
            }
        }

        public async Task DisposeAsync()
        {
            await Task.Run(() => Dispose()).ConfigureAwait(false);
        }

        public void Dispose()
        {
            var appDomains = TestCases.Select(x => x.AppDomain).Distinct().ToList();

            foreach (var appDomain in appDomains)
            {
                AppDomain.Unload(appDomain);
            }
        }
    }
}