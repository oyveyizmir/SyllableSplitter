/*
функцію=фун-кці-ю кц - impossible onset кц
давньогрецької=дав-ньог-рець-ко-ї гр-don't split
юхтман=юх-тман тм-impossible?
александра=а-лек-сан-дра нд - don't split
зустріч=зус-тріч don't split ст
андромахою=ан-дро-ма-хо-ю don't split нд
шістнадцята=шіс-тнад-ця-та don't split ст
безсмертних=без-смер-тних don't split рт
військо=вій-сько don't split йсь
ахейських=а-хей-ських don't split йсь
криводзьобих don't split дз,дзь,дж
андрій=ан-дрій
божественній=бо-жес-твен-ній
полководці=по-лко-вод-ці
шіс-тде-сят
фун-кці-йі
не-вол-ьни-цю
test:      
суп-ро-во-див

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
än-de-ru-ngen ng at the beginning?
sol-lte compound ll-t
in-ter-pun-kti-on kt not at the beginning
schrän-kter nk-t
freun-dli-che nd-l
kon-nte nn-t
knö-pfen kn only at the beginning of a word
fi-nger-chen ng never at the beginning of a syllable
zit-ter-nden ng never at the begining of a syllable rn-d
or-den-tlich nt-l
kon-trol-liert nt-r
fün-ftes nün-ftig nf-t
den-kli-chen nk-l
ng-
 */
using System;
using System.Collections.Generic;
using System.IO;
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

            syllableBreaker.ProcessClusters();
            PrintConsonantClusters(syllableBreaker);
        }

        private static void PrintConsonantClusters(SyllableBreaker syllableBreaker)
        {
            using (var writer = new StreamWriter("clusters_by_count.txt", false, Encoding.UTF8))
            {
                foreach (var clusterText in syllableBreaker.ClustersByCount)
                {
                    var cluster = syllableBreaker.ConsonantClusters[clusterText];
                    writer.Write($"{clusterText} ({cluster.Words.Count}): ");
                    WriteWords(writer, cluster.Words);
                    writer.WriteLine();
                }
            }

            using (var writer = new StreamWriter("clusters_by_length.txt", false, Encoding.UTF8))
            {
                foreach (var clusterText in syllableBreaker.ClustersByLength)
                {
                    var cluster = syllableBreaker.ConsonantClusters[clusterText];
                    writer.Write($"{clusterText} ({cluster.Words.Count}): ");
                    WriteWords(writer, cluster.Words);
                    writer.WriteLine();
                }
            }
        }

        private static void WriteWords(StreamWriter writer, List<List<Syllable>> words)
        {
            bool first = true;
            foreach (var word in words)
            {
                if (!first)
                    writer.Write(", ");
                writer.Write(string.Join("-", word));
                first = false;
            }
        }

        private static void PrintSyllables(List<Syllable> syllables)
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
