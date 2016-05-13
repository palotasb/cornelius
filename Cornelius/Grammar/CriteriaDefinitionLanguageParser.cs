using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Cornelius.Criteria.Expression;
using Antlr.Runtime;

namespace Cornelius.Grammar
{
    partial class CriteriaDefinitionLanguageParser
    {
        public static MatchGroup Parse(string file)
        {
            using (FileStream stream = File.OpenRead(file))
            {
                ANTLRInputStream antlrStream = new ANTLRInputStream(stream);
                CriteriaDefinitionLanguageLexer lexer = new CriteriaDefinitionLanguageLexer(antlrStream);
                CommonTokenStream tokenStream = new CommonTokenStream(lexer);
                CriteriaDefinitionLanguageParser parser = new CriteriaDefinitionLanguageParser(tokenStream);
                return parser.definition();
            }
        }
    }
}
