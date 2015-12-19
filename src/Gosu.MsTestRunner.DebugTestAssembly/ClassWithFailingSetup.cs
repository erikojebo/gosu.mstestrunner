using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gosu.MsTestRunner.DebugTestAssembly
{
    [TestClass]
    public class ClassWithFailingSetup
    {
        private bool _failTearDown;

        [TestInitialize]
        public void SetUp()
        {
            _failTearDown = false;

            throw new InvalidOperationException("Configured to fail in the setup");
        }

        [TestMethod]
        public void Succeeding_test_with_failing_setup()
        {
            Assert.AreEqual(1, 1);
        }

        [TestMethod]
        public void Failing_test_with_failing_setup()
        {
            Assert.AreEqual(2, 1);
        }

        [Ignore]
        [TestMethod]
        public void Ignored_test_with_failing_setup()
        {
            Assert.AreEqual(1, 1);
        }

        [TestMethod]
        public void Test_with_both_failing_setup_and_teardown()
        {
            Assert.AreEqual(1, 1);    
        }

        [TestCleanup]
        public void TearDown()
        {
            if (_failTearDown)
                throw new InvalidOperationException("Configured to fail in the tear down");
        }
    }
}