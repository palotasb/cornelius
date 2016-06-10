using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cornelius.Data;
using Cornelius.IO.Primitives;

namespace Cornelius
{
    static class Builder
    {
        public static IEnumerable<Specialization> ExtractSpecializations(Import import)
        {
            Log.Write("Specializációk feldolgozása...");
            Log.EnterBlock();
            foreach (var primitive in import.Specializations)
            {
                Specialization specialization = new Specialization();
                specialization.EducationProgram = primitive.EducationProgram;
                specialization.Name = primitive.Name;
                specialization.SpecializationGroup = primitive.SpecializationGroup;
                specialization.MaxRatio = primitive.MaxRatio;
                specialization.MinRatio = primitive.MinRatio;
                specialization.Capacity = primitive.Capacity;
                Log.Write(specialization.EducationProgram + " - " + specialization.Name + " (" + specialization.SpecializationGroup + "): " + specialization.MinRatio + "-" + specialization.MaxRatio.ToString("#.00%"));
                yield return specialization;
            }
            Log.LeaveBlock();
        }

        public static IEnumerable<SpecializationGrouping> ExtractSpecializationGroupings(Import import, IEnumerable<Specialization> specializations)
        {
            Log.Write("Specializációcsoportok feldolgozása...");
            Log.EnterBlock();
            var specGroups = specializations
                .GroupBy(spec => spec.SpecializationGroup)
                .Select(specGroup => new SpecializationGrouping(specGroup));
            foreach (var primitive in import.SpecializationGroupings)
            {
                SpecializationGrouping specGroup;
                if (specGroups.Any(sg => sg.Key == primitive.Name))
                    specGroup = specGroups.First(sg => sg.Key == primitive.Name);
                else
                    continue;
                specGroup.Capacity = primitive.Capacity;
                specGroup.MinRatio = primitive.MinRatio;
                specGroup.MaxRatio = primitive.MaxRatio;
                specGroup.PreSpecializationCourseGroup = primitive.PreSpecializationCourseGroup;
                specGroup.EducationProgram = primitive.EducationProgram;
                yield return specGroup;
            }
            Log.LeaveBlock();
        }

        public static IEnumerable<Student> ExtractStudents(Import import)
        {
            Log.Write("Hallgatók feldolgozása...");
            Log.EnterBlock();
            foreach (var primitive in import.Identities.GroupBy(identity => identity.Key))
            {
                Log.Write(primitive.Key + ":");
                Log.EnterBlock(" => ");
                Student student = Builder.MergeIdentities(primitive);
                Log.Write(student.Name);
                Log.Write("Eredeti képzés: " + student.OriginalEducationProgram);
                Log.Write("Jelentkezés szerinti: " + student.EducationProgram);

                student.Choices = import.Choices
                    .Where(choice => choice.Key == student.OriginKey)
                    .OrderBy(choice => choice.Number)
                    .Select(choice => choice.Name).ToArray();
                Log.Write("Jelentkezések: " + student.Choices.Length);

                student.Courses = Builder.ExtractCourses(import.Entries.Where(entry => entry.Key == student.OriginKey)).ToArray();
                Log.Write("Kurzusok: " + student.Courses.Length);
                Log.LeaveBlock();
                yield return student;
            }
            Log.LeaveBlock();
        }

        private static Student MergeIdentities(IEnumerable<XIdentity> primitives)
        {
            XIdentity first = primitives.First();
            if (primitives.Count() > 1 && !(
                    primitives.All(primitive => primitive.BaseName == first.BaseName) &&
                    primitives.All(primitive => primitive.BaseNeptun == first.BaseNeptun) &&
                    primitives.All(primitive => primitive.BaseEducationProgram == first.BaseEducationProgram) &&
                    primitives.All(primitive => primitive.OriginalEducationProgram == first.OriginalEducationProgram) &&
                    primitives.All(primitive => primitive.EffectiveSemester == first.EffectiveSemester
                )))
            {
                Log.Write("HIBALEHETŐSÉG: A hallgató adatai között ütközés van.");
            }

            Student student = new Student();
            student.Neptun = first.BaseNeptun;
            student.Name = first.BaseName;
            student.EducationProgram = first.BaseEducationProgram;
            student.OriginalEducationProgram = first.OriginalEducationProgram;
            student.EffectiveSemester = first.EffectiveSemester;
            student.Emails = primitives
                .Select(primitive => primitive.Email)
                .Distinct()
                .ToArray();

            return student;
        }

        public static IEnumerable<Course> ExtractCourses(IEnumerable<XEntry> entries)
        {
            // Harmadik vizsga szűrés
            Semester currentSemester = Semester.FromDate(DateTime.Now);
            var threeExams = from entry in entries
                             group entry by entry.Code into course
                             where course.Count(e => e.EntryType == EntryType.Vizsga && e.Semester == currentSemester) >= 3
                             select course;

            if (threeExams.Count() > 1)
            {
                entries = entries.Except(threeExams.SelectMany(course => course
                    .Where(entry => entry.EntryType == EntryType.Vizsga && entry.Semester == currentSemester)
                    .OrderBy(entry => entry.EntryDate)
                    .Take(2)
                ));
                Log.Write("A hallgató több mint egy harmadik vizsgán vett részt.");
            }

            foreach (var byCode in entries.GroupBy(entry => entry.Code))
            {
                Course course = new Course();
                var relevant = byCode.OrderByDescending(entry => entry.EntryDate);

                // Alapadatok kinyerése
                var first = relevant.First();
                course.Code = first.Code;
                course.Name = first.Name;

                // Aláírások kigyűjtése és aláírás meglétének eldöntése
                var signatures = relevant.Where(entry => entry.EntryType == EntryType.Alairas);
                course.HasSignature = signatures.Count() > 0 && signatures.First().EntryValue == EntryValue.Alairva;

                // Követelmény eldöntése
                EntryType? requirement = relevant.Select(e => e.Requirement).Where(r => r.HasValue).FirstOrDefault();

                // Szigorlat esetén 5 kreditnek számít
                course.Credit = (requirement.HasValue ? requirement == EntryType.Szigorlat : first.EntryType == EntryType.Szigorlat) ? 5 : first.Credit;

                if (requirement.HasValue ? requirement == EntryType.Alairas : course.Credit == 0)
                {
                    // Ha aláírás a követelmény, vagy ha 0 kredites a tárgy,
                    // akkor elég az aláírás a teljesítéshez, és végeztünk is
                    course.HasCompleted = course.HasSignature;
                }
                else
                {
                    // Tárgyteljesítések lekérése
                    var completions = relevant.Where(entry => entry.EntryType == EntryType.Evkozi || entry.EntryType == EntryType.Szigorlat || entry.EntryType == EntryType.Vizsga);
                    if (completions.Count() > 0)
                    {
                        // A teljesítés alapján eldöntjük, tényleg teljesítette-e
                        first = completions.First();
                        course.Grade = (int)first.EntryValue;
                        course.HasCompleted = course.Grade > 1;

                        // Nem mindig van a Neptun bejegyzésnél félév, ezt ilyenkor nekünk kell előállítani a dátumból.
                        course.EffectiveSemester = first.Semester.HasValue ? first.Semester.Value : Semester.FromDate(first.EntryDate);
                    }
                }

                yield return course;
            }
        }
    }
}
