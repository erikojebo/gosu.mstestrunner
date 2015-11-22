using System.Reflection;

namespace Gosu.MsTestRunner.Core.Listeners.Events
{
    public class TestPass : TestRunEvent
    {
        public TestPass(MethodInfo testMethod) : base(testMethod)
        {
        }
    }
}