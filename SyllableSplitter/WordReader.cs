using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SyllableSplitter
{
    class WordReader
    {
        private enum State
        {
            LineRequired,
            WordRequired,
            EndOfInput
        }

        //private static final Logger logger = LogManager.getLogger();

        private TextReader scanner;
        private Regex wordPattern = new Regex("\\b\\p{L}[-'\\p{L}+]*\\b", RegexOptions.Compiled);
        private Match match;
        private State state = State.LineRequired;

        public WordReader(string inputFile = null)
        {
            if (inputFile == null)
                scanner = Console.In;
            else
                scanner = new StreamReader(inputFile);
        }

        public string GetNextWord()
        {
            switch (state)
            {
                case State.LineRequired:
                    string line = ReadLine();
                    if (line != null)
                    {
                        match = wordPattern.Match(line);
                        return GetNextWord(State.WordRequired);
                    }
                    else
                        return GetNextWord(State.EndOfInput);

                case State.WordRequired:
                    if (match.Success)
                    {
                        string word = match.Value;
                        match = match.NextMatch();
                        return word;
                    }
                    else
                        return GetNextWord(State.LineRequired);

                case State.EndOfInput:
                    return null;

                default:
                    throw new InvalidOperationException("Invalid state " + state);
            }
        }

        private string GetNextWord(State newState)
        {
            state = newState;
            return GetNextWord();
        }

        private string ReadLine()
        {
            string line = scanner.ReadLine()?.Trim();
            if (line == null)
                return null;

            //logger.debug("Read line: " + line);

            if (line.EndsWith("-"))
                return line.Substring(0, line.Length - 1) + ReadLine();

            return line;
        }

        public void Close()
        {
            scanner.Close();
        }
    }
}
