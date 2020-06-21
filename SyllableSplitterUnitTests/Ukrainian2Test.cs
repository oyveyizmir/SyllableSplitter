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
        [TestCaseSource("GetTestData", new object[] { "Ukrainian2-гл-гл.csv" })]
        public void TestГлухийГлухий(string word, string expected) => BreakWord(word, expected);

        [Test]
        [TestCaseSource("GetTestData", new object[] { "Ukrainian2-гл-сон.csv" })]
        public void TestГлухийСонорний(string word, string expected) => BreakWord(word, expected);

        [Test]
        [TestCaseSource("GetTestData", new object[] { "Ukrainian2-дз-сон.csv" })]
        public void TestДзвінкийСонорний(string word, string expected) => BreakWord(word, expected);

        [Test]
        [TestCaseSource("GetTestData", new object[] { "Ukrainian2-сон-гл.csv" })]
        public void TestСонорнийГлухий(string word, string expected) => BreakWord(word, expected);

        [Test]
        [TestCaseSource("GetTestData", new object[] { "Ukrainian2-сон-сон.csv" })]
        public void TestСонорнийСонорний(string word, string expected) => BreakWord(word, expected);

        [Test]
        [TestCaseSource("GetTestData", new object[] { "Ukrainian2-гл-гл-сон.csv" })]
        public void TestГлухийГлухийСонорний(string word, string expected) => BreakWord(word, expected);

        [Test]
        [TestCaseSource("GetTestData", new object[] { "Ukrainian2-дз-дз.csv" })]
        public void TestДзвінкийДзвінкий(string word, string expected) => BreakWord(word, expected);

        [Test]
        [TestCaseSource("GetTestData", new object[] { "Ukrainian2-дз-гл.csv" })]
        public void TestДзвінкийГлухий(string word, string expected) => BreakWord(word, expected);

        [Test]
        [TestCaseSource("GetTestData", new object[] { "Ukrainian2-сон-дз.csv" })]
        public void TestСонорнийДзвінкий(string word, string expected) => BreakWord(word, expected);
    }
}
