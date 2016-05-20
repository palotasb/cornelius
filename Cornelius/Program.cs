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
        public const string CONFIG_DIRECTORY = "Beállítások";
        public const string INPUT_DIRECTORY = "Bemenet";
        public const string OUTPUT_DIRECTORY = "Kimenet";

        static void Main(string[] args)
        {
            Import import = Import.LoadAllFiles();
            List<Student> students = Builder.ExtractStudents(import).ToList();
            List<Specialization> specializations = Builder.ExtractSpecializations(import).ToList();
            Evaluator.ProcessStudents(students, import.Exceptions);
            
            var eduPrograms = students
                .Where(student => student.Result)
                .OrderBy(student => student.Round)
                .ThenByDescending(student => student.Result.Avarage)
                .GroupBy(student => student.EduProgramCode);

            foreach (var eduProgram in eduPrograms)
            {
                int count = eduProgram.Count();
                Dictionary<string, Specialization> shorthandDictionary = specializations
                    .Where(specialization => specialization.Group == eduProgram.Key)
                    .ToDictionary(specialization => specialization.Name.Remove(15));

                Log.Write("Maximális szakiránylétszámok a " + eduProgram.Key + " képzésen:");
                Log.EnterBlock(" => ");
                foreach (var specialization in shorthandDictionary.Values)
                {
                    specialization.Capacity = (int)Math.Round(count * specialization.Ratio);
                    Log.Write(specialization.Name + ": " + specialization.Capacity);
                }
                Log.LeaveBlock();

                Log.Write("Besorolás a " + eduProgram.Key + " képzésen:");
                Log.EnterBlock();
                foreach (var student in eduProgram)
                {
                    Log.Write(student.OriginKey + ":");
                    Log.EnterBlock(" => ");
                    foreach (var choice in student.Choices)
                    {
                        var specialization = shorthandDictionary[choice.Remove(15)];
                        Log.Write((Array.IndexOf<string>(student.Choices, choice) + 1) + ". hely: " + specialization.Name);
                        if (!specialization.Full || specialization.Minimum == student.Result.Avarage && specialization.Round == student.Round)
                        {
                            Log.Write("Besorolva.");
                            specialization += student;
                            specialization.Round = student.Round;
                            break;
                        }
                    }
                    if (student.Specialization == null)
                    {
                        Log.Write("Sikertelen besorolás.");
                    }
                    Log.LeaveBlock();
                }
                Log.LeaveBlock();
            }

            Log.Write("Eredmény lemezre írása...");
            Log.EnterBlock();
            if (!Directory.Exists(Program.OUTPUT_DIRECTORY))
            {
                Directory.CreateDirectory(Program.OUTPUT_DIRECTORY);
            }
            Export.ExportDatabases(students);
            Export.ExportReports(students, specializations);
            Export.ExportCreditStatistics(students);
            Log.LeaveBlock();
        }
    }
}
