using Gosu.MsTestRunner.Core.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gosu.MsTestRunner.Tests
{
    [TestClass]
    public class PrettifyIdentifierTests
    {
        [TestMethod]
        public void Capitalizes_first_letter_of_camel_case_identifier()
        {
            Assert.AreEqual("An identifier", "anIdentifier".PrettifyIdentifier());
        }

        [TestMethod]
        public void Capitalizes_single_letter_identifier()
        {
            Assert.AreEqual("A", "a".PrettifyIdentifier());
        }

        [TestMethod]
        public void Replaces_underscore_with_space()
        {
            Assert.AreEqual("A test name", "A_test_name".PrettifyIdentifier());
        }

        [TestMethod]
        public void Breaks_down_camel_case_identifier_into_words_separated_by_spaces()
        {
            Assert.AreEqual("A test name", "ATestName".PrettifyIdentifier());
        }

        [TestMethod]
        public void Leaves_upper_case_acronyms_intact()
        {
            Assert.AreEqual("Create XML test", "CreateXMLTest".PrettifyIdentifier());
            Assert.AreEqual("Create IP address", "CreateIPAddress".PrettifyIdentifier());
        }
    }
}
