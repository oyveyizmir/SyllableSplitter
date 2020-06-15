using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyllableSplitter
{
    public class LetterCluster
    {
        public string Text;
        public List<string> Letters;
        public List<List<Syllable>> Words = new List<List<Syllable>>();

        public LetterCluster(string text, List<string> letters, List<Syllable> syllables)
        {
            Text = text;
            Letters = letters;
            Words.Add(syllables);
        }
    }
}
