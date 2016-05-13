using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Cornelius.Criteria.Workflow;
using Antlr.Runtime;

namespace Cornelius.Grammar
{
    partial class WorkflowDefinitionLanguageParser
    {
        public static List<AbstractWorkflow> Parse(string file)
        {
            using (FileStream stream = File.OpenRead(file))
            {
                ANTLRInputStream antlrStream = new ANTLRInputStream(stream);
                WorkflowDefinitionLanguageLexer lexer = new WorkflowDefinitionLanguageLexer(antlrStream);
                CommonTokenStream tokenStream = new CommonTokenStream(lexer);
                WorkflowDefinitionLanguageParser parser = new WorkflowDefinitionLanguageParser(tokenStream);
                return parser.definition();
            }
        }
    }
}
