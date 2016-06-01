using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cornelius.Data;

namespace Cornelius.Criteria.Workflow
{
    /// <summary>
    /// A 2016 nyarától kezdődően érvényes feltételek szerinti besorolást elvégző workflow. Kritériumok:
    /// (a) legalább 90 kredit a teljes mintatantervből,
    /// (b) első két szemeszter tárgyai*,
    /// (c) legalább 20 kredit a 3. szemeszter tárgyaiból*,
    /// (d) szigorlat és a specializációelőkészítő tárgy.
    /// (* Két tankörivel kiváltható feltétel az egyik 1 tárgy illetve 5 kredit értékben.)
    /// A rangosátlag az első négy félév kreditsúlyozott tárgyeredményei osztva 120 kreditponttal.
    /// </summary>
    class Bachelor2016Workflow : AbstractWorkflow
    {
        /// <summary>
        /// Az első két szemeszter kötelező tárgyainak csoportneve.
        /// </summary>
        private const string Semester12CourseGroup = "Kötelező_1_2_Félév";
        /// <summary>
        /// A harmadik és későbbi félévek tárgyainak csoportneve.
        /// </summary>
        private const string Semester3xCourseGroup = "Kötelező_3_x_Félév";
        /// <summary>
        /// String prefix a specializáció-előkészítő tárgyak csoportnevéhez.
        /// </summary>
        private const string PreSpecCourseGroup = "Előkészítő_";
        // TODO feldolgozni, hogy a specializációkódok (tanszékrövidítések) stimmelnek-e.
        /// <summary>
        /// A specializációelőkészítő kategórák neve és a specializációk rövidítése, amire az előkészítő tárgyak vonatkoznak.
        /// </summary>
        private readonly Tuple<string, string[]>[] PreSpecCourses = {
            Tuple.Create("Beágy",       new string[] { "AUT", "MIT", "IIT" }),
            Tuple.Create("Infokomm",    new string[] { "HIT", "TMIT", "HVT" }),
            Tuple.Create("EETT",        new string[] { "EET", "ETT" }),
            Tuple.Create("Villenerg",   new string[] { "VET" })
        };
        /// <summary>
        /// A szigorlati tárgy csoportneve.
        /// </summary>
        private const string ExamGroup = "Szigorlat";
        /// <summary>
        /// Kötelezően választható (compulsory humanities) tárgyak csoportneve.
        /// </summary>
        private const string CompHumCourseGroup = "KötelezőenVálasztható";
        /// <summary>
        /// Szabadon választható tárgyak csoportneve.
        /// </summary>
        private const string FreeChoiceCourseGroup = "SzabadonVálasztható";
        /// <summary>
        /// A tanköri foglalkozások csoportneve.
        /// </summary>
        private const string StudyGroupGroup = "Tanköri";

        /// <summary>
        /// Ezt a függvényt hívja meg az <see cref="Evaluator"/> osztály
        /// a hallgató kritériumkiértékeléséhez. Ellenőrzi, hogy a szabályzat 2. § (5) a-d) szerinti követelmények a
        /// 2. § (6) szerinti mentesség figyelembe vételével teljesülnek-e.
        /// </summary>
        /// <param name="student">A hallgató, akinek a kritériumait ellenőrizzük.</param>
        protected override void ProcessFinalResult(Student student)
        {
            // 2. § (6) feldolgozása
            // Tanköri alapján mentesség meghatározása
            bool Has26Exemption = Determine26Exemption(student);
            bool Uses26Exemption = false;
            Log.Write("Info: A hallgató rendelkezik engedményre jogosító két tankörivel: " + (Has26Exemption ? "igen" : "nem"));

            // 2. § (5) b) feldolgozása.
            // Ez direkt van előbb az a)-nál, mert ez hívja meg az AbstractWorkflow.ProcessCourseRequirements függvényt,
            // ami a student.Result-ot létrehozza.
            ProcessCompulsoryCourses(student, Has26Exemption, ref Uses26Exemption);

            // 2. § (5) a) feldolgozása.
            ProcessMinimumCreditRequirement(student);

            // 2. § (5) c) feldolgozása..
            ProcessSemester3Requirements(student, Has26Exemption, ref Uses26Exemption);

            // 2. § (5) d) feldolgozása.
            ProcessExamRequirements(student);
        }

        /// <summary>
        /// A minimumkreditkövetelmény feldolgozása. Ez 90 kredit a mintatantervből (bármelyik félév).
        /// </summary>
        /// <param name="student">A hallgató, akin az ellenőrzés elvégzendő.</param>
        private void ProcessMinimumCreditRequirement(Student student)
        {
            // TODO ellenőrizni, hogy ez nem számolja-e túl a kötválokat vagy szabválokat. Hint: valószínűleg de.

            // A hallgató mintatantervi tárgyai az egyes diszjunk csoportokban.
            var semester12Courses = FilterCriteriaCourses(student, Semester12CourseGroup);
            var semester3xCourses = FilterCriteriaCourses(student, Semester3xCourseGroup);
            var compHumCourses = FilterCriteriaCourses(student, CompHumCourseGroup);
            var freeCourses = FilterCriteriaCourses(student, FreeChoiceCourseGroup);
            var preSpecCourses = (from psc in PreSpecCourses // Ez a szépség a teljesített specializációelőkészítő tárgyakat szedi össze
                                  select FilterCriteriaCourses(student, PreSpecCourseGroup + psc.Item1))
                                  .Aggregate((IEnumerable<Course>) new Course[] { }, (a, b) => a.Concat(b));

            // A hallgató összes mintatantervi tárgya.
            var allCourses =
                       (semester12Courses)
                .Concat(semester3xCourses)
                .Concat(compHumCourses)
                .Concat(freeCourses)
                .Concat(preSpecCourses);

            // Kreditek összeszámolása
            var credits = (from c in allCourses
                           select c.Credit)
                          .Sum();

            // Eredmény tárolása
            var result = new Result("Kreditkritérium", 90 <= (int)credits);
            if (!result)
            {
                result.Weight = 90 - (int)credits;
            }
            Log.Write("Info: A kreditkritérium elfogadva: " + (result.Value ? "igen" : "nem"));

            student.Result.Weight += result.Weight;
            student.Result += result;
            student.Result.Value = student.Result.Value && result.Value;
        }

        /// <summary>
        /// Kötelező tárgyak feldolgozása. Ezek az első két szemeszter mintatantervi kötelező és kötelezően választható tárgyai.
        /// Tankörivel egy tantárgy kiváltható.
        /// </summary>
        /// <param name="student">A hallgató, akin az ellenőrzés elvégzendő.</param>
        private void ProcessCompulsoryCourses(Student student, bool has26Exemption, ref bool uses26Exemption)
        {
            // Kurzuskritériumok feldolgozása. Ezek a CRD kritériumfájlban foglalt kötelező kurzusok.
            // A végén a student.Result tagváltozó fog értékkel rendelkezni.
            // Ez azt ellenőrzi, hogy az első két szemeszter tárgyai megvannak-e.
            if (CourseCriteria != null)
            {
                // Kötelező tárgyak ellenőrzése az első két félévből.
                base.ProcessCourseRequirements(student);

                // Mintatanterv szerinti kötelezően választhatók ellenőrzése
                // Teljesített kötelezően választhatók leszűrése
                var CompHumCourses = FilterCriteriaCourses(student, CompHumCourseGroup);
                // Összehasonlítás az előírt minimummal szakonként
                var CompHumResult = new Result(CompHumCourseGroup,
                    SummaCriteria.FirstOrDefault(gr => gr.Identifier == CompHumCourseGroup).Amount <= CompHumCourses.Count());
                // Súly kiszámítása (súly == hiányzó kritériumok száma)
                if (CompHumResult)
                {
                    CompHumResult.Weight = 0;
                }
                else
                {
                    CompHumResult.Weight = SummaCriteria.FirstOrDefault(gr => gr.Identifier == CompHumCourseGroup).Amount - CompHumCourses.Count();
                }
                // Kötvál aleredmény tárolása
                student.Result.Weight += CompHumResult.Weight;
                student.Result += CompHumResult;
                student.Result.Value = student.Result.Value && CompHumResult.Value;

                // Ellenőrizzük, hogy a tanköri kedvezmény használható-e.
                if (student.Result.Value == false)
                {
                    Log.Write("Info: A hallgatónak nincs meg minden tantárgya az első két félévből.");
                    if (has26Exemption && !uses26Exemption)
                    {   // Kiváltható a kritérium
                        Log.Write("Info: Az erre vonatkozó mentességet ki tudja használni a hallgató.");
                        uses26Exemption = true;
                        student.Result.Weight -= 1;
                        if (student.Result.Weight <= 0)
                        {
                            student.Result.Value = true;
                        }
                    }
                }
                Log.Write("Info: Első két félév kötelező tárgyai kritérium elfogadva: " + (student.Result.Value ? "igen" : "nem"));
            }
            else
            {
                Log.Write("Figyelmeztetés: A kurzuskritérium a kritériumellenőrzés során nincs beállítva.");
            }
        }
        
        /// <summary>
        /// A harmadik félévre vonatkozó követelmény feldolgozása. Ez 20 kredit, de 5 alól tankörivel felmentés kapható.
        /// </summary>
        /// <param name="student">A hallgató, akin az ellenőrzés elvégzendő.</param>
        private void ProcessSemester3Requirements(Student student, bool has26Exemption, ref bool uses26Exemption)
        {
            // TODO ellenőrizni, hogy ide pontosan mely tárgyak számíthatnak bele (pl. specelőkészítő? 4. félév kötelezői?)
            var semester3xCourses = FilterCriteriaCourses(student, Semester3xCourseGroup);
            var credits = (from c in semester3xCourses
                           select c.Credit)
                           .Sum();
            var result = new Result("Harmadik félévre vonatközó kritérium", 20 <= (int)credits);
            if (!result)
            {
                Log.Write("Info: A hallgatónak nincs 20 kreditje a harmadik félév mintatantervi tárgyaiból.");
                result.Weight = 20 - (int)credits;
                if (has26Exemption && !uses26Exemption)
                {
                    Log.Write("Info: 5 kreditnyi mentességet tud használni a hallgató.");
                    uses26Exemption = true;
                    credits += 5;
                    result.Weight -= 5;
                    if (result.Weight <= 0)
                    {
                        result.Weight = 0;
                        result.Value = true;
                    }
                }
            }
            Log.Write("Info: A harmadik szemeszterből húsz kredit kritérium elfogadva: " + (result.Value ? "igen" : "nem"));

            // Eredmény tárolása
            student.Result.Weight += result.Weight;
            student.Result += result;
            student.Result.Value = student.Result.Value && result.Value;
        }

        /// <summary>
        /// A szigorlati követelmény ellenőrzése.
        /// </summary>
        /// <param name="student">A hallgató, akin az ellenőrzés elvégzendő.</param>
        private void ProcessExamRequirements(Student student)
        {
            // A megkövetelt mennyiség a WD (workflow definition) fájlban van rögzítve.
            // Ha abban nem szerepel, akkor a követelmény 0.
            var result = new Result("Szigorlat",
                SummaCriteria.FirstOrDefault(gr => gr.Identifier == ExamGroup).Amount <= FilterCriteriaCourses(student, ExamGroup).Count());
        }

        /// <summary>
        /// Meghatározza, hogy a hallgatónak jár-e a 2. § (6) alapján a mentesség az (5) b) vagy c) alól.
        /// </summary>
        /// <param name="student"></param>
        /// <returns></returns>
        protected bool Determine26Exemption(Student student)
        {
            return 2 <= FilterCriteriaCourses(student, StudyGroupGroup).Count();
        }

        /// <summary>
        /// A hallgatók kritériumellenőrzését végző függvény. Ezt hívja meg az
        /// <see cref="Evaluator.ProcessStudents(IEnumerable{Student}, IEnumerable{IO.Primitives.XBase})"/> függvény.
        /// </summary>
        /// <param name="student">A hallgató, akinek a kritériummegfelelőségét ellenőrizni kell.</param>
        /// <param name="exception">Igaz, ha a hallgató fel van mentve a kritériumok alól.</param>
        public override void ProcessStudent(Student student, bool exception)
        {
            Log.Write(student.OriginKey + ":");
            Log.EnterBlock(" => ");
            ProcessFinalResult(student);
            if (exception)
            {
                Log.Write("Kritériumok alól felmentve.");
                student.Result.Value = true;
            }
            if (student.Result) Log.Write(student.Round + ". körben besorolható.");
            else Log.Write("Nem besorolható.");
            Log.LeaveBlock();
        }
    }
}
