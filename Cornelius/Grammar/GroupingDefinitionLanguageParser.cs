using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Cornelius.Criteria.Credit;
using Antlr.Runtime;

namespace Cornelius.Grammar
{
    partial class GroupingDefinitionLanguageParser
    {
        public static Grouping Parse(string file)
        {
            using (FileStream stream = File.OpenRead(file))
            {
                ANTLRInputStream antlrStream = new ANTLRInputStream(stream);
                GroupingDefinitionLanguageLexer lexer = new GroupingDefinitionLanguageLexer(antlrStream);
                CommonTokenStream tokenStream = new CommonTokenStream(lexer);
                GroupingDefinitionLanguageParser parser = new GroupingDefinitionLanguageParser(tokenStream);
                return parser.definition();
            }
        }
    }
}
