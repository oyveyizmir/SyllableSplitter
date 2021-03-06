﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SyllableSplitter;

namespace SyllableSplitterUnitTests
{
    [TestFixture]
    public class UkrainianTest : RuleTestBase
    {
        protected override string RuleFileName => "ukrainian.json";

        [Test]
        [TestCaseSource("GetTestData", new object[] { "Ukrainian.csv" })]
        public void TestBreakWord(string word, string expected) => BreakWord(word, expected);
    }
}
