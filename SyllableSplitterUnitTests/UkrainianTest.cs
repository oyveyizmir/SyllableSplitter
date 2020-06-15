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
    public class UkrainianTest
    {
        public static IEnumerable<string[]> GetTestData()
        {
            string fileName = "SyllableSplitterUnitTests.Data.Ukrainian.csv";
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fileName))
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

        [Test]
        [TestCaseSource("GetTestData")]
        public void BreakWord(string word, string expected)
        {
            string directoryPath = Path.GetDirectoryName(typeof(UkrainianTest).Assembly.Location);
            string filePath = Path.Combine(directoryPath, "Config", "ua.json");
            var conf = Configuration.Read(filePath);
            var syllableBreaker = new SyllableBreaker(conf);

            List<Syllable> syllables = syllableBreaker.BreakWord(word);
            string result = string.Join("-", syllables);

            Assert.AreEqual(expected, result);
        }
    }
}
