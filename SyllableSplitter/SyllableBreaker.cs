﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyllableSplitter
{
    class SyllableBreaker
    {
        private readonly Configuration conf;
        private readonly string[] compoundConsonants;
        private readonly string[] compoundVowels;
        private readonly string[] prefixes;
        private readonly char[] separators;

        public SyllableBreaker(Configuration conf)
        {
            this.conf = conf;
            compoundConsonants = conf.CompoundConsonants?.Split(',');
            compoundVowels = conf.CompoundVowels?.Split(',');
            prefixes = conf.Prefixes?.Split(',');
            separators = conf.Separators?.ToCharArray();
        }

        public List<Syllable> BreakWord(string word)
        {
            if (conf.Separators != null)
            {
                string[] parts = word.Split(separators);
                var syllables = new List<Syllable>();
                foreach (string part in parts)
                    syllables.AddRange(BreakPrefixedWord(part));
                return syllables;
            }
            else
                return BreakPrefixedWord(word);
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
                        syllables.AddRange(BreakWord(word));
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

            return syllables;
        }

        private bool IsVowel(string letter)
        {
            if (compoundVowels != null && compoundVowels.Contains(letter))
                return true;

            if (conf.Vowels != null)
                foreach (char ch in conf.Vowels)
                    if (ch.ToString() == letter)
                        return true;

            return false;
        }

        private bool IsConsonant(string letter)
        {
            if (compoundConsonants != null && compoundConsonants.Contains(letter))
                return true;

            if (conf.Consonants != null)
                foreach (char ch in conf.Consonants)
                    if (ch.ToString() == letter)
                        return true;

            return false;
        }

        private List<string> SplitIntoLetters(string word)
        {
            var letters = new List<string>();

            for (int i = 0; i < word.Length;)
            {
                bool found = false;

                if (compoundConsonants != null)
                    foreach (string letter in compoundConsonants)
                        if ((i + letter.Length <= word.Length) && word.Substring(i, letter.Length) == letter)
                        {
                            letters.Add(letter);
                            i += letter.Length;
                            found = true;
                            break;
                        }

                if (found)
                    continue;

                if (compoundVowels != null)
                    foreach (string letter in compoundVowels)
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
