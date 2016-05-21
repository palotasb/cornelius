using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cornelius.Data;

namespace Cornelius.Criteria.Workflow
{
    /// <summary>
    /// A 2016 nyarától kezdődően érvényes feltételek szerinti besorolást elvégző workflow.
    /// </summary>
    class Bachelor2016Workflow : AbstractWorkflow
    {
        protected override void ProcessFinalResult(Student student)
        {
            throw new NotImplementedException();
        }
    }
}
