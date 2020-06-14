using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SyllableSplitter
{
    class SyllableBreaker
    {
        private readonly Configuration conf;
        private readonly string[] consonants;
        private readonly string[] vowels;
        private readonly string[] prefixes;
        private readonly char[] separators;
        private readonly Dictionary<string, string[]> letterClasses = new Dictionary<string, string[]>();
        private readonly List<RewriteRule> rewriteRules = new List<RewriteRule>();

        public readonly Dictionary<string, LetterCluster> ConsonantClusters = new Dictionary<string, LetterCluster>();
        public List<string> ClustersByCount;
        public List<string> ClustersByLength;

        public SyllableBreaker(Configuration conf)
        {
            if (conf.Vowels == null)
                throw new ArgumentException("Vowels");
            if (conf.Consonants == null)
                throw new ArgumentException("Consonants");

            this.conf = conf;

            if (conf.LetterClasses != null)
                foreach (var letterClass in conf.LetterClasses)
                {
                    var classTerms = letterClass.Split('=');
                    if (classTerms.Length != 2)
                        throw new ArgumentException($"Cannot parse letter class {letterClass}");
                    var className = classTerms[0];
                    var classItems = classTerms[1];
                    if (letterClasses.ContainsKey(className))
                        throw new ArgumentException($"Duplicate letter class {className}");
                    letterClasses.Add(className, classItems.Split(','));
                }

            vowels = ExpandLetterClasses(conf.Vowels).Split(',');
            consonants = ExpandLetterClasses(conf.Consonants).Split(',');
            prefixes = conf.Prefixes?.Split(',');
            separators = conf.Separators?.ToCharArray();
            

            if (conf.RewriteRules != null)
                foreach (var rule in conf.RewriteRules)
                {
                    var ruleTerms = rule.Split('/');
                    switch (ruleTerms.Length)
                    {
                        case 2:
                            ParseRule(ruleTerms[0], ruleTerms[1], null);
                            break;
                        case 3:
                            ParseRule(ruleTerms[0], ruleTerms[1], ruleTerms[2]);
                            break;
                        default:
                            throw new ArgumentException($"Invalid rewrite rule {rule}");
                    }
                }
        }

        public List<Syllable> BreakWord(string word)
        {
            if (conf.Separators != null)
            {
                string[] parts = word.Split(separators);
                var syllables = new List<Syllable>();
                foreach (string part in parts)
                    syllables.AddRange(BreakPrefixedWord(RewriteLetters(part)));
                return syllables;
            }
            else
                return BreakPrefixedWord(RewriteLetters(word));

            //count clusters here? partial words are written for prefixes
        }

        public void ProcessClusters()
        {
            ClustersByCount = ConsonantClusters.OrderByDescending(p => p.Value.Words.Count).Select(x => x.Key).ToList();
            ClustersByLength = ConsonantClusters.OrderByDescending(p => p.Value.Letters.Count).Select(x => x.Key).ToList();

            /*ClustersByLength = ConsonantClusters.Select(p => new { p.Key, p.Value.Letters.Count })
                .OrderByDescending(s => s.Key.Length).Select(x => x.Key).ToList();*/
        }

        private void ParseRule(string searchPattern, string replacementTerm, string searchContext)
        {
            string searchClass = null;
            string replacementClass = FindLetterClass(replacementTerm); //TODO: make sure there's only 1 replacement class
            if (replacementClass == null)
            {
                foreach (var letterClass in letterClasses)
                {
                    string classItemsPatten = "(" + string.Join("|", letterClass.Value) + ")";
                    searchPattern = searchPattern.Replace(letterClass.Key, classItemsPatten);
                }
            }
            else
            {
                searchClass = FindLetterClass(searchPattern); //TODO: make sure there's only 1 search class
                if (searchClass == null)
                    throw new ArgumentException($"No search letter class found in rewrite rule {searchPattern}/{replacementTerm}/{searchContext}");
                //TODO: check then search and replacement classes have the same length or replacement is bigger
                string classItemsPatten = "(?<LetterClass>" + string.Join("|", letterClasses[searchClass]) + ")";
                searchPattern = searchPattern.Replace(searchClass, classItemsPatten);
            }

            if (searchContext == null)
            {
                searchContext = "(?<Replacement>" + searchPattern + ")";
            }
            else
            {
                if (searchContext.IndexOf('_') < 0)
                    throw new ArgumentException($"No substitution character (_) in rule {searchPattern}/{replacementTerm}/{searchContext}");

                searchContext = searchContext.Replace("_", "(?<Replacement>" + searchPattern + ")");
                foreach (var letterClass in letterClasses)
                {
                    string classItemsPatten = "(" + string.Join("|", letterClass.Value) + ")";
                    searchContext = searchContext.Replace(letterClass.Key, classItemsPatten);
                }
            }

            var rewriteRule = new RewriteRule(searchContext, searchClass, replacementTerm, replacementClass);
            rewriteRules.Add(rewriteRule);
        }

        private string ExpandLetterClasses(string str)
        {
            foreach (var letterClass in letterClasses)
            {
                string classItemsPatten = string.Join(",", letterClass.Value);
                str = str.Replace(letterClass.Key, classItemsPatten);
            }

            return str;
        }

        private string RewriteLetters(string word)
        {
            foreach (var rule in rewriteRules)
            {
                word = rule.SearchRegex.Replace(word, m =>
                    {
                        string insert;

                        if (rule.ReplacementClass == null)
                            insert = rule.ReplacementTerm;
                        else
                        {
                            var replacementLetterClass = letterClasses[rule.ReplacementClass];
                            var searchLetterClass = letterClasses[rule.SearchClass];
                            var foundLetter = m.Groups["LetterClass"].Value;
                            var letterIndex = Array.FindIndex(searchLetterClass, x => x == foundLetter);
                            var replacementLetter = replacementLetterClass[letterIndex];
                            insert = rule.ReplacementTerm.Replace(rule.ReplacementClass, replacementLetter);
                        }

                        int insertIndex = m.Groups["Replacement"].Index - m.Groups[0].Index;
                        string replacement = m.Groups[0].Value.Remove(insertIndex, m.Groups["Replacement"].Length);
                        replacement = replacement.Insert(insertIndex, insert);
                        return replacement;
                    });
            }

            return word;
        }

        private string FindLetterClass(string term)
        {
            foreach (var letterClass in letterClasses.Keys)
                if (term.IndexOf(letterClass) >= 0)
                    return letterClass;
            return null;
        }

        private List<Syllable> BreakPrefixedWord(string word)
        {
            if (prefixes != null)
                foreach (string prefix in prefixes)
                    if (word.StartsWith(prefix))
                    {
                        var syllables = Break(prefix);
                        word = word.Substring(prefix.Length, word.Length - prefix.Length);
                        if (word.Length == 0)
                            return new List<Syllable>();
                        syllables.AddRange(BreakPrefixedWord(word));
                        return syllables;
                    }

            return Break(word);
        }

        private List<Syllable> Break(string word)
        {
            if (word.Length == 0)
                return new List<Syllable>();

            var syllables = new List<Syllable>();
            var letters = SplitIntoLetters(word);

            Syllable syllable = new Syllable();
            syllables.Add(syllable);

            foreach (string letter in letters)
            {
                if (IsVowel(letter))
                {
                    if (syllable.Nucleus == null)
                        syllable.Nucleus = letter;
                    else
                    {
                        syllable = new Syllable() { Nucleus = letter };
                        syllables.Add(syllable);
                    }
                }
                else if (IsConsonant(letter))
                {
                    if (syllable.Nucleus == null)
                        syllable.Onset.Add(letter);
                    else
                        syllable.Coda.Add(letter);
                }
                else
                    throw new ArgumentException($"Unrecognizable letter {letter} in word {word}");
            }

            foreach (var syl in syllables)
            {
                if (syl.Onset.Count > 0)
                {
                    string onset = string.Join(" ", syl.Onset);
                    if (ConsonantClusters.ContainsKey(onset))
                        ConsonantClusters[onset].Words.Add(syllables);
                    else
                        ConsonantClusters[onset] = new LetterCluster(onset, syl.Onset, syllables);
                }

                if (syl.Coda.Count > 0)
                {
                    string coda = string.Join(" ", syl.Coda);
                    if (ConsonantClusters.ContainsKey(coda))
                        ConsonantClusters[coda].Words.Add(syllables);
                    else
                        ConsonantClusters[coda] = new LetterCluster(coda, syl.Coda, syllables);
                }
            }

            /*var clusters = new Dictionary<string, LetterCluster>();

            foreach (var syl in syllables)
            {
                if (syl.Onset.Count > 0)
                {
                    string onset = string.Join(" ", syl.Onset);
                    if (!clusters.ContainsKey(onset))
                        ConsonantClusters[onset] = new LetterCluster(onset, syl.Onset);
                }

                if (syl.Coda.Count > 0)
                {
                    string coda = string.Join(" ", syl.Coda);
                    if (!clusters.ContainsKey(coda))
                        ConsonantClusters[coda] = new LetterCluster(coda, syl.Coda);
                }
            }*/

            for (int i = 0; i < syllables.Count - 1; i++)
            {
                Syllable current = syllables[i];
                Syllable next = syllables[i + 1];

                if (current.Coda.Count == 1)
                {
                    next.Onset = current.Coda;
                    current.Coda = new List<string>();
                }
                else if (current.Coda.Count > 1)
                {
                    next.Onset = current.Coda.GetRange(1, current.Coda.Count - 1);
                    current.Coda = current.Coda.GetRange(0, 1);
                }
            }

            /*foreach (LetterCluster cluster in clusters.Values)
            {
                if (ConsonantClusters.ContainsKey(cluster.Text))
                    ConsonantClusters[cluster.Text].Words.Add(syllables);
                else
                    ConsonantClusters[onset] = new LetterCluster(onset, syl.Onset);

                cluster.Text = 
                if (syl.Onset.Count > 0)
                {
                    string onset = string.Join(" ", syl.Onset);
                    if (ConsonantClusters.ContainsKey(onset))
                        ConsonantClusters[onset].Words.Add(syllables);
                    else
                        ConsonantClusters[onset] = new LetterCluster(onset, syl.Onset);
                }

                if (syl.Coda.Count > 0)
                {
                    string coda = string.Join(" ", syl.Coda);
                    if (ConsonantClusters.ContainsKey(coda))
                        ConsonantClusters[coda].Words.Add(word);
                    else
                        ConsonantClusters[coda] = new List<string>() { word };
                }
            }*/

            return syllables;
        }

        private bool IsVowel(string letter) => vowels != null && vowels.Contains(letter);

        private bool IsConsonant(string letter) => consonants != null && consonants.Contains(letter);

        private List<string> SplitIntoLetters(string word)
        {
            var letters = new List<string>();

            for (int i = 0; i < word.Length;)
            {
                bool found = false;

                if (consonants != null)
                    foreach (string letter in consonants)
                        if ((i + letter.Length <= word.Length) && word.Substring(i, letter.Length) == letter)
                        {
                            letters.Add(letter);
                            i += letter.Length;
                            found = true;
                            break;
                        }

                if (found)
                    continue;

                if (vowels != null)
                    foreach (string letter in vowels)
                        if ((i + letter.Length <= word.Length) && word.Substring(i, letter.Length) == letter)
                        {
                            letters.Add(letter);
                            i += letter.Length;
                            found = true;
                            break;
                        }

                if (found)
                    continue;

                letters.Add(word[i++].ToString());
            }

            return letters;
        }
    }
}
