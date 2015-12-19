using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gosu.MsTestRunner.DebugTestAssembly
{
    [TestClass]
    public class Class1
    {
        private bool _failTearDown;

        [TestInitialize]
        public void SetUp()
        {
            _failTearDown = false;
        }

        [TestMethod]
        public void Successful_test()
        {
            Assert.AreEqual(1, 1, "supposed to succeed");
        }
        
        [ExpectedException(typeof(InvalidCastException))]
        [TestMethod]
        public void Successful_test_with_expected_exception()
        {
            throw new InvalidCastException("expected exception");
        }

        [TestMethod]
        public void Failing_test()
        {
            Assert.AreEqual(1, 2, "Supposed to fail");
        }

        [ExpectedException(typeof(InvalidCastException))]
        [TestMethod]
        public void Failing_test_with_expected_exception()
        {
            throw new InvalidOperationException("Exception that was not expected to be thrown");
        }

        [Ignore]
        [TestMethod]
        public void Ignored_test()
        {
            
        }

        [TestMethod]
        public void Succeeding_test_with_failing_tear_down()
        {
            _failTearDown = true;

            Assert.IsFalse(false);
        }

        [TestCleanup]
        public void TearDown()
        {
            if (_failTearDown)
                throw new InvalidOperationException("Configured to fail in the tear down");
        }

        [TestMethod]
        public void Test_with_a_very_long_name_that_will_probably_reach_the_right_side_of_the_test_runner_window_where_the_names_are_shown()
        {
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void CamelCaseTestName()
        {
            Assert.IsTrue(true);
        }
    }
}
