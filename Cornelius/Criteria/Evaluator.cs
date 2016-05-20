using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cornelius.Criteria.Workflow;
using Cornelius.Data;
using Cornelius.Grammar;

namespace Cornelius.Criteria
{
    /// <summary>
    /// Ez az osztály betölti, tárolja és társítja a kritériumrendszereket, majd
    /// tömegével kiértékeli a hallgatókon őket.
    /// </summary>
    static class Evaluator
    {
        /// <summary>
        /// A betöltött kritériumrendszerek.
        /// </summary>
        private static List<AbstractWorkflow> Workflows;

        /// <summary>
        /// Létrehozáskor betöltődik a beállításfájl, ami
        /// definiálja a kritériumrendszereket.
        /// </summary>
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

        /// <summary>
        /// Hallgatók tömeges kritériummegfeleltetése.
        /// </summary>
        /// <param name="students">A hallgatók listája.</param>
        /// <param name="exceptions">A kritériumkivételek listája.</param>
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

        /// <summary>
        /// A hallgatóra vonatkozó kritériumrendszer kikeresése. Speciális
        /// esetben az átsorolás figyelmen kívül hagyásával.
        /// </summary>
        /// <param name="student">A hallgató.</param>
        /// <param name="original">Beállítja, hogy az eredeti képzése szerinti követelményeket nézzük-e a hallgatónál.</param>
        /// <returns></returns>
        public static AbstractWorkflow Match(Student student, bool original)
        {
            return Evaluator.Workflows.First(workflow => workflow.Match(student, original));
        }
    }
}
