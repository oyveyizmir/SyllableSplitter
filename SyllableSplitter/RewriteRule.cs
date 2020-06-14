using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SyllableSplitter
{
    class RewriteRule
    {
        public Regex SearchRegex;
        public string SearchClass;
        public string ReplacementTerm;
        public string ReplacementClass;

        public RewriteRule(string searchPattern, string searchClass, string replacemetnTerm, string replacementClass)
        {
            SearchRegex = new Regex(searchPattern, RegexOptions.Compiled);
            SearchClass = searchClass;
            ReplacementTerm = replacemetnTerm;
            ReplacementClass = replacementClass;
        }
    }
}
