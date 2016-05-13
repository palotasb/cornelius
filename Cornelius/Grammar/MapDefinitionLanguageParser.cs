using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Cornelius.IO.Mapping;
using Antlr.Runtime;

namespace Cornelius.Grammar
{
    partial class MapDefinitionLanguageParser
    {
        public static Map Parse(string file)
        {
            using (FileStream stream = File.OpenRead(file))
            {
                ANTLRInputStream antlrStream = new ANTLRInputStream(stream);
                MapDefinitionLanguageLexer lexer = new MapDefinitionLanguageLexer(antlrStream);
                CommonTokenStream tokenStream = new CommonTokenStream(lexer);
                MapDefinitionLanguageParser parser = new MapDefinitionLanguageParser(tokenStream);
                return parser.definition();
            }
        }
    }
}
