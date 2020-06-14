using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyllableSplitter
{
#pragma warning disable CS0649

    class Configuration
    {
        public string Vowels;
        public string Consonants;
        public string Prefixes;
        public string Separators;
        public string[] LetterClasses;
        public string[] RewriteRules;
        public string[] SplitRules;

        public static Configuration Read(string file)
        {
            using (var reader = new StreamReader(file))
            {
                string configText = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<Configuration>(configText);
            }
        }
    }
}
