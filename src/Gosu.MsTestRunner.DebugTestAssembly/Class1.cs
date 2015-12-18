﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gosu.MsTestRunner.DebugTestAssembly
{
    [TestClass]
    public class Class1
    {
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
    }
}
