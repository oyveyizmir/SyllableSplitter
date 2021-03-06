﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SyllableSplitter.Extensions;

namespace SyllableSplitter
{
    public class SyllableBreaker
    {
        private readonly Configuration conf;
        private readonly string[] consonants;
        private readonly string[] vowels;
        private readonly string[] alphabet;
        private readonly string[] prefixes;
        private readonly char[] separators;
        private readonly Dictionary<string, string[]> letterClasses = new Dictionary<string, string[]>();
        private readonly List<RewriteRule> rewriteRules = new List<RewriteRule>();
        private readonly List<Regex> splitRules = new List<Regex>();

        public int MinClusterSize { get; set; } = 1;

        public readonly Dictionary<string, LetterCluster> ConsonantClusters = new Dictionary<string, LetterCluster>();
        public List<string> ClustersByCount;
        public List<string> ClustersByLength;

        public bool TraceOn { get; set; }

        public SyllableBreaker(Configuration conf, bool traceOn = false)
        {
            TraceOn = traceOn;

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
            alphabet = consonants.Concat(vowels).ToArray();
            prefixes = conf.Prefixes?.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            separators = conf.Separators?.ToCharArray();

            if (conf.RewriteRules != null)
                foreach (var rule in conf.RewriteRules)
                {
                    var ruleTerms = rule.Split('/');
                    switch (ruleTerms.Length)
                    {
                        case 2:
                            ParseRewriteRule(ruleTerms[0], ruleTerms[1], null);
                            break;
                        case 3:
                            ParseRewriteRule(ruleTerms[0], ruleTerms[1], ruleTerms[2]);
                            break;
                        default:
                            throw new ArgumentException($"Invalid rewrite rule {rule}");
                    }
                }

            if (conf.SplitRules != null)
                foreach (var rule in conf.SplitRules)
                {
                    var ruleTerms = rule.Split('/');
                    //TODO: check rule format
                    string codeTerm = ruleTerms[0];
                    string onsetTerm = ruleTerms[1];
                    if (codeTerm == "" && onsetTerm == "")
                        continue;

                    string codaPattern = codeTerm != "" ? "(?<termCoda>" + ToLetterRegex(codeTerm) + ")" : "";
                    string onsetPattern = onsetTerm != "" ? "(?<termOnset>" + ToLetterRegex(onsetTerm) + ")" : "";

                    var splitRuleRegex = new Regex("^" + codaPattern + onsetPattern + "$", RegexOptions.Compiled);
                    splitRules.Add(splitRuleRegex);

                    if (TraceOn)
                        Console.WriteLine($"Split rule {rule} => {splitRuleRegex}");
                }
        }

        public List<Syllable> BreakWord(string word)
        {
            if (TraceOn)
                Console.WriteLine($"Splitting word {word}");

            var syllables = new List<Syllable>();

            if (conf.Separators != null && conf.Separators.Length != 0)
            {
                string[] parts = word.Split(separators);
                foreach (string part in parts)
                    BreakPrefixedWord(RewriteLetters(part), syllables);
            }
            else
                BreakPrefixedWord(RewriteLetters(word), syllables);

            if (TraceOn)
                Console.WriteLine($"Word {word} split into syllables {ToString(syllables)}");

            return syllables;
            //count clusters here? partial words are written for prefixes
        }

        public void ProcessClusters()
        {
            ClustersByCount = ConsonantClusters.OrderByDescending(p => p.Value.Words.Count).Select(x => x.Key).ToList();
            ClustersByLength = ConsonantClusters.OrderByDescending(p => p.Value.Letters.Count).Select(x => x.Key).ToList();

            /*ClustersByLength = ConsonantClusters.Select(p => new { p.Key, p.Value.Letters.Count })
                .OrderByDescending(s => s.Key.Length).Select(x => x.Key).ToList();*/
        }

        public static string ToString(List<Syllable> syllables)
        {
            var sb = new StringBuilder();

            bool first = true;
            foreach (var syllable in syllables)
            {
                if (!first)
                    sb.Append("-");
                sb.Append(syllable);
                first = false;
            }

            return sb.ToString();
        }

        private static string ToString(List<string> letters) => string.Concat(letters);

        private string ToLetterRegex(string pattern)
        {
            var sb = new StringBuilder();

            
            for (int i = 0; i < pattern.Length;)
            {
                //Expand letter classes into (?:\[au\]|\[eu\]|\[oi\])
                if (letterClasses.Parse(pattern, ref i, x => x.Key, x =>
                    {
                        string letterClassPattern = LetterClassToRegex(x.Value);
                        sb.Append(letterClassPattern);
                    }))
                    continue;

                //Expand letters into (?:\[ng\])
                if (alphabet.Parse(pattern, ref i, x => sb.Append(@"(?:\[" + x + @"\])")))
                    continue;

                sb.Append(pattern[i++].ToString());
            }

            sb.Replace(".", @"(?:\[[^\]]+\])");
            return sb.ToString();
        }

        private string LetterClassToRegex(string[] letterClass) => "(?:" + string.Join("|", letterClass.Select(x => @"\[" + x + @"\]")) + ")";

        private void ParseRewriteRule(string searchPattern, string replacementTerm, string searchContext)
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

        private void BreakPrefixedWord(string word, List<Syllable> syllables)
        {
            string prefix = FindPrefix(word);
            if (prefix != null)
            {
                Break(prefix, syllables);
                word = word.Remove(0, prefix.Length);
                BreakPrefixedWord(word, syllables);
            }
            else
                Break(word, syllables);
        }

        private string FindPrefix(string word)
        {
            if (prefixes == null)
                return null;

            foreach (string prefix in prefixes)
                if (word.StartsWith(prefix))
                {
                    if (TraceOn)
                        Console.WriteLine($"Found prefix {prefix}");
                    return prefix;
                }

            return null;
        }

        private void Break(string word, List<Syllable> syllables)
        {
            if (TraceOn)
                Console.WriteLine($"Breaking word {word}");

            if (word.Length == 0)
                return;

            var letters = SplitIntoLetters(word);

            int firstSyllableIndex = syllables.Count;
            Syllable syllable = new Syllable();
            syllables.Add(syllable);
            if (syllables.Count >= 2)
                syllables[syllables.Count - 2].Next = syllable;

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
                        syllables[syllables.Count - 2].Next = syllable;
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

            for (int i = firstSyllableIndex; i < syllables.Count; i++)
            {
                Syllable syl = syllables[i];

                if (syl.Onset.Count >= MinClusterSize)
                {
                    string onset = string.Join(" ", syl.Onset);
                    if (ConsonantClusters.ContainsKey(onset))
                    {
                        if (!ConsonantClusters[onset].Words.Contains(syllables))
                            ConsonantClusters[onset].Words.Add(syllables);
                    }
                    else
                        ConsonantClusters[onset] = new LetterCluster(onset, syl.Onset, syllables);
                }

                if (syl.Coda.Count >= MinClusterSize)
                {
                    string coda = string.Join(" ", syl.Coda);
                    if (ConsonantClusters.ContainsKey(coda))
                    {
                        if (!ConsonantClusters[coda].Words.Contains(syllables))
                            ConsonantClusters[coda].Words.Add(syllables);
                    }
                    else
                        ConsonantClusters[coda] = new LetterCluster(coda, syl.Coda, syllables);
                }
            }

            for (int i = firstSyllableIndex; i < syllables.Count - 1; i++)
            {
                Syllable current = syllables[i];
                Syllable next = syllables[i + 1];
                string cluster = BracketLetters(current.Coda);

                if (TraceOn)
                    Console.WriteLine($"Splitting cluster {cluster}");

                for (int j = 0; j < splitRules.Count; j++)
                {
                    var rule = splitRules[j];

                    var match = rule.Match(cluster);
                    if (match.Success)
                    {
                        string coda = match.Groups["termCoda"].Value;
                        current.Coda = UnbracketLetters(coda);

                        string onset = match.Groups["termOnset"].Value;
                        next.Onset = UnbracketLetters(onset);

                        if (TraceOn)
                            Console.WriteLine($"Rule {conf.SplitRules[j]} matched: {coda}/{onset}");

                        break;
                    }
                }
            }
        }

        private string BracketLetters(List<string> letters)
        {
            var sb = new StringBuilder();
            foreach (var letter in letters)
                sb.Append("[" + letter + "]");
            return sb.ToString();
        }

        private List<string> UnbracketLetters(string bracketedLetters)
        {
            var word = new List<string>();
            int openingBracket = 0;

            while (openingBracket < bracketedLetters.Length)
            {
                if (bracketedLetters[openingBracket] != '[')
                    throw new ArgumentException($"Opening backet is expected in {bracketedLetters} at position {openingBracket}");

                int closingBacket = bracketedLetters.IndexOf(']', openingBracket + 1);
                if (closingBacket < 0)
                    throw new ArgumentException($"Closing backet is expected in {bracketedLetters} after position {openingBracket}");

                string letter = bracketedLetters.Substring(openingBracket + 1, closingBacket - openingBracket - 1);
                word.Add(letter);

                openingBracket = closingBacket + 1;
            }

            return word;
        }

        private bool IsVowel(string letter) => vowels != null && vowels.Contains(letter);

        private bool IsConsonant(string letter) => consonants != null && consonants.Contains(letter);

        private List<string> SplitIntoLetters(string word)
        {
            var letters = new List<string>();

            for (int i = 0; i < word.Length;)
                if (!alphabet.Parse(word, ref i, x => letters.Add(x)))
                    letters.Add(word[i++].ToString());

            return letters;
        }
    }
}
