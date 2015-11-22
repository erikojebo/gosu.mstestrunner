using System;
using System.IO;
using System.Reflection;

namespace Gosu.MsTestRunner.Core.Listeners.Events
{
    public class TestRunEvent
    {
        public TestRunEvent(MethodInfo testMethod)
        {
            DateTime = DateTime.Now;
            TestMethod = testMethod;
        }

        public DateTime DateTime;
        public MethodInfo TestMethod;

        public string TestMethodName => TestMethod.Name;
        public string TestClassName => TestMethod.DeclaringType?.Name;
        public string TestAssemblyName => Path.GetFileNameWithoutExtension(TestMethod.DeclaringType?.Assembly.Location);
    }
}