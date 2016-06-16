using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Cornelius.Data;
using Cornelius.Criteria;

namespace Cornelius
{
    class Program
    {
        public const string CONFIG_DIRECTORY = "Config";
        public const string INPUT_DIRECTORY = "Bemenet";
        public const string OUTPUT_DIRECTORY = "Kimenet";

        static void Main(string[] args)
        {            
            /* Megjegyzés: a programban a Specialization osztály a szabályzat terminológiájában valójában ágazatot jelöl, de a programban erre
             * specializációként (néha ágazatként) hivatkozunk. A SpecializationGrouping osztály a szabályzat terminológiájában egy specializációt
             * jelöl, de a programban erre specializációcsoportként hivatkozunk.
             * 
             * A besoroló program:
             *  - Importáljuk a beállításokat, a besorolási paramétereket, tárgylistákat, hallgatókat, tárgyeredményeket és jelentkezéseket.
             *  - A hallgatókon mind elvégezzük a kritériumellenőrzést és az átlagszámítást a nekik megfelelő Workflow (kritériumrendszer) alapján.
             *  - Lefuttatjuk a besorolási algoritmust (lásd Placement.Algorithm).
             *  - Lemezre mentjük az eredményeket.
            **/

            var import = Import.LoadAllFiles();
            var allStudents = Builder.ExtractStudents(import).ToList();
            var specializations = Builder.ExtractSpecializations(import).ToList();
            var specGroupings = Builder.ExtractSpecializationGroupings(import, specializations).ToList();
            Evaluator.ProcessStudents(allStudents, import.Exceptions, specGroupings);
            
            var studentsByEduProgram = allStudents.GroupBy(student => student.EducationProgram);
            var specGroupingsByEduProgram = specGroupings.GroupBy(sg => sg.EducationProgram);
            
            // Képzési programokon való iteráció
            foreach (var studentsInProgram in studentsByEduProgram)
            {
                Log.Write(string.Format("Besorolás a(z) {0} képzésen", studentsInProgram.Key));
                Log.EnterBlock();
                var algorithm = new Placement.Algorithm(studentsInProgram, specGroupingsByEduProgram.First(sg => sg.Key == studentsInProgram.Key));
                IEnumerable<Tuple<Student, Specialization>> placements;
                if (algorithm.Run(out placements))
                {
                    foreach (var tuple in placements)
                    {
                        var spec = tuple.Item2;
                        spec += tuple.Item1;
                    }
                }
                Log.LeaveBlock();
            }

            Log.Write("Eredmény lemezre írása...");
            Log.EnterBlock();
            if (!Directory.Exists(Program.OUTPUT_DIRECTORY))
            {
                Directory.CreateDirectory(Program.OUTPUT_DIRECTORY);
            }
            Export.ExportDatabases(allStudents);
            Export.ExportReports(allStudents, specializations, specGroupings);
            Export.ExportCreditStatistics(allStudents);
            Log.LeaveBlock();
        }
    }
}
