/*
функцію=фун-кці-ю кц - impossible onset кц
давньогрецької=дав-ньог-рець-ко-ї гр-don't split
юхтман=юх-тман тм-impossible?
александра=а-лек-сан-дра нд - don't split
зустріч=зус-тріч don't split ст
андромахою=ан-дро-ма-хо-ю don't split нд
п'ятнадцята=п-ят-над-ця-та override 'я->йа
шістнадцята=шіс-тнад-ця-та don't split ст
безсмертних=без-смер-тних don't split рт
військо=вій-сько don't split йсь
ахейських=а-хей-ських don't split йсь
криводзьобих don't split дз,дзь,дж
андрій=ан-дрій
божественній=бо-жес-твен-ній

offensichtliche=of-fen-sich-tli-che
blondkopf=blon-dkopf
handschlitten=han-dschlit-ten
gedanken=ge-da-nken
knöpfen=knöp-fen
hauptsächlich=haup-tsäch-lich
hellgrau=hel-lgrau
künstlichen=kün-stli-chen
außerordentlich=au-ßer-or-den-tlich
erster=er-ster
do not split: pf, st at the beginning and after prefix
aufmerksamen=auf-mer-ksa-men
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyllableSplitter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            if (args.Length < 1)
            {
                Console.WriteLine("Usage: SyllableSplitter.exe <ConfigFile> [<TextFile>]");
                return;
            }

            Configuration conf = Configuration.Read(args[0]);
            var syllableBreaker = new SyllableBreaker(conf);
            WordReader reader = new WordReader(args.Length >= 2 ? args[1] : null);

            string word;
            var processedWords = new HashSet<string>();
            while ((word = reader.GetNextWord()) != null)
            {
                word = word.ToLower();
                if (processedWords.Contains(word))
                    continue;
                processedWords.Add(word);
                Console.Write($"{word}=");
                try
                {
                    var syllables = syllableBreaker.BreakWord(word);
                    PrintSyllables(syllables);
                }
                catch(ArgumentException e)
                {
                    Console.WriteLine($"Error: {e.Message}");
                }
            }

            reader.Close();
        }

        static void PrintSyllables(List<Syllable> syllables)
        {
            bool first = true;
            foreach (var syllable in syllables)
            {
                if (!first)
                    Console.Write("-");
                Console.Write(syllable);
                first = false;
            }
            Console.WriteLine();
        }
    }
}
