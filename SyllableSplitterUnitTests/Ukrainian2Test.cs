using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SyllableSplitter;

namespace SyllableSplitterUnitTests
{
    [TestFixture]
    public class Ukrainian2Test : RuleTestBase
    {
        protected override string RuleFileName => "ukrainian2.json";

        [Test]
        [TestCaseSource("GetTestData", new object[] { "Ukrainian2.csv" })]
        public void TestBreakWord(string word, string expected) => BreakWord(word, expected);
    }
}
