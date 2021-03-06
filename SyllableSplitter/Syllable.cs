﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyllableSplitter
{
    public class Syllable
    {
        public List<string> Onset = new List<string>();
        public string Nucleus;
        public List<string> Coda = new List<string>();
        public Syllable Next;

        public string Cluster => string.Concat(Coda) + Next != null ? string.Concat(Next.Onset) : string.Empty;

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var str in Onset)
                sb.Append(str);
            sb.Append(Nucleus);
            foreach (var str in Coda)
                sb.Append(str);
            return sb.ToString();
        }
    }
}
