using NUnit.Framework;
using SyllableSplitter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SyllableSplitterUnitTests
{
    public abstract class RuleTestBase
    {
        private SyllableBreaker syllableBreaker;

        protected static IEnumerable<string[]> GetTestData(string fileName)
        {
            string fullFileName = "SyllableSplitterUnitTests.Data." + fileName;
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullFileName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var parts = line.Split(',');
                    parts = parts.Take(2).Select(x => x.Trim()).ToArray();
                    yield return parts;
                }
            }
        }

        protected abstract string RuleFileName { get; }

        [OneTimeSetUp]
        public void SetUpTestFixture()
        {
            string directoryPath = Path.GetDirectoryName(typeof(RuleTestBase).Assembly.Location);
            string filePath = Path.Combine(directoryPath, "Config", RuleFileName);
            var conf = Configuration.Read(filePath);
            syllableBreaker = new SyllableBreaker(conf);
        }

        protected void BreakWord(string word, string expected)
        {
            List<Syllable> syllables = syllableBreaker.BreakWord(word);
            string result = string.Join("-", syllables);

            Assert.AreEqual(expected, result);
        }
    }
}
