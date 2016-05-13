using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cornelius.Criteria.Workflow;
using Cornelius.Data;
using Cornelius.Grammar;

namespace Cornelius.Criteria
{
    /*
     * Ez az osztály betölti, tárolja és társítja a kritériumrendszerekt, majd
     * tömegével kiértékeli a hallgatókon őket.
     */
    static class Evaluator
    {
        /*
         * A betöltött kritériumrendszerek.
         */
        private static List<AbstractWorkflow> Workflows;

        /*
         * Létrehozáskor betöltődik a beállítás fájl, ami
         * definiálja a kritériumrendszereket.
         */
        static Evaluator()
        {
            Evaluator.Workflows = WorkflowDefinitionLanguageParser.Parse(Path.Combine(Program.CONFIG_DIRECTORY, "kepzesek.wd"));
            Log.Write("Kritériumrendszerek betöltése...");
            Log.EnterBlock(" => ");
            foreach (var workflow in Evaluator.Workflows)
            {
                Log.Write(workflow.CourseCriteria.Name);
            }
            Log.LeaveBlock();
        }

        /*
         * Hallgatók tömeges kritériummegfeleltetése.
         */
        public static void ProcessStudents(IEnumerable<Student> students, IEnumerable<Cornelius.IO.Primitives.XBase> exceptions)
        {
            Log.Write("Kritériumrendszerek kiértékelése...");
            Log.EnterBlock();
            foreach (var student in students)
            {
                Evaluator.Match(student, false).ProcessStudent(student, exceptions.Any(s => s.Key == student.OriginKey));
            }
            Log.LeaveBlock();
        }

        /*
         * A hallgatóra vonatkozó kritériumrendszer kikeresése. Speciális
         * esetben az átsorolás figyelmen kívül hagyásával.
         */
        public static AbstractWorkflow Match(Student student, bool original)
        {
            return Evaluator.Workflows.First(workflow => workflow.Match(student, original));
        }
    }
}
