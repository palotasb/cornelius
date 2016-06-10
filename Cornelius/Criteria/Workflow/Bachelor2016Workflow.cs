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
    /// (b) első két szemeszter tárgyai *,
    /// (c) legalább 20 kredit a 3. szemeszter tárgyaiból *,
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
        /// A harmadik félév tárgyainak csoportneve.
        /// </summary>
        private const string Semester3CourseGroup = "Kötelező_3_Félév";

        /// <summary>
        /// A negyedik félév tárgyainak csoportneve.
        /// </summary>
        private const string Semester4CourseGroup = "Kötelező_4_Félév";

        /// <summary>
        /// Az ötödik és későbbi félév tárgyainak csoportneve.
        /// </summary>
        private const string Semester5xCourseGroup = "Kötelező_5_x_Félév";

        /// <summary>
        /// String prefix a specializáció-előkészítő tárgyak csoportnevéhez.
        /// </summary>
        private const string PreSpecCourseGroup = "Előkészítő_";

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
        /// A hallgatók kritériumellenőrzését végző függvény. Ezt hívja meg az
        /// <see cref="Evaluator.ProcessStudents(IEnumerable{Student}, IEnumerable{IO.Primitives.XBase})"/> függvény.
        /// </summary>
        /// <param name="student">A hallgató, akinek a kritériummegfelelőségét ellenőrizni kell.</param>
        /// <param name="exception">Igaz, ha a hallgató fel van mentve a kritériumok alól.</param>
        public override void ProcessStudent(Student student, bool exception, IEnumerable<SpecializationGrouping> specializationGroupings)
        {
            // TODO átgondolni, hogy a workflow definition fájlban kell-e SummaCriteria az összes tárgycsoporthoz vagy úgy se használjuk...
            Log.Write(student.OriginKey + ":");
            Log.EnterBlock(" => ");
            // Hiányzó specializációk hozzáadása a jelentkezéshez.
            AddMissingSpecializations(student, specializationGroupings);
            // El nem érhető specilaizációk eltávolítása.
            RemoveUnattainableSpecializations(student, specializationGroupings);
            // Kritériumok ellenőrzése. (2. § szerint)
            ProcessFinalResult(student, specializationGroupings);
            // 3. § (3) feldolgozása, rangsorszámítás.
            CalculateRank(student, specializationGroupings);
            if (exception)
            {
                Log.Write("Kritériumok alól felmentve.");
                student.Result.Value = true;
            }
            if (student.Result) Log.Write(student.Round + ". körben besorolható.");
            else Log.Write("Nem besorolható.");
            Log.LeaveBlock();
        }

        /// <summary>
        /// Ezt a függvényt hívja meg a <see cref="ProcessStudent(Student, bool, IEnumerable{SpecializationGrouping})"/>
        /// a kritériumellenőrzéshez. Ellenőrzi, hogy a szabályzat 2. § (5) a-d) szerinti követelmények a
        /// 2. § (6) szerinti mentesség figyelembe vételével teljesülnek-e.
        /// </summary>
        /// <param name="student">A hallgató, akinek a kritériumait ellenőrizzük.</param>
        protected override void ProcessFinalResult(Student student, IEnumerable<SpecializationGrouping> specializationGroupings)
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
            ProcessMinimumCreditRequirement(student, specializationGroupings);
            // 2. § (5) c) feldolgozása..
            ProcessSemester3Requirements(student, Has26Exemption, ref Uses26Exemption);
            // 2. § (5) d) feldolgozása.
            ProcessExamRequirements(student);
            // A 2. § (7) feldolgozása külön történik az el nem érhető specializációk eltávolításával a jelentkezési sorból.
        }

        /// <summary>
        /// A minimumkreditkövetelmény feldolgozása. Ez 90 kredit a mintatantervből (bármelyik félév).
        /// </summary>
        /// <param name="student">A hallgató, akin az ellenőrzés elvégzendő.</param>
        private void ProcessMinimumCreditRequirement(Student student, IEnumerable<SpecializationGrouping> specializationGroupings)
        {
            // A hallgató mintatantervi _kötelező_ tárgyai az egyes diszjunkt csoportokban.
            var semester12Courses = FilterCriteriaCourses(student, Semester12CourseGroup);
            var semester3Courses = FilterCriteriaCourses(student, Semester3CourseGroup);
            var semester4Courses = FilterCriteriaCourses(student, Semester4CourseGroup);
            var semester5xCourses = FilterCriteriaCourses(student, Semester5xCourseGroup);
            var preSpecCourses = (from specGroup in specializationGroupings // Ez a szépség a teljesített specializációelőkészítő tárgyakat szedi össze
                                  select FilterCriteriaCourses(student, PreSpecCourseGroup + specGroup.PreSpecializationCourseGroup))
                                  .SelectMany(courses => courses);

            // A hallgató összes mintatantervi tárgya a kötválok és szabválok kivételével.
            var allCourses =   (semester12Courses)
                        .Concat(semester3Courses)
                        .Concat(semester4Courses)
                        .Concat(semester5xCourses)
                        .Concat(preSpecCourses);

            // Kreditek összeszámolása (kötelezőek)
            var credits = (from c in allCourses
                           select c.Credit)
                          .Sum();

            // Kötválok és szabválok számítása max 10 kredit értékig.
            credits += Math.Min(10, FilterCriteriaCourses(student, FreeChoiceCourseGroup).Sum(c => c.Credit));
            credits += Math.Min(10, FilterCriteriaCourses(student, CompHumCourseGroup).Sum(c => c.Credit));

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
        /// Tankörivel egy tantárgy kiváltható, a kiváltást a függvény jelzi.
        /// </summary>
        /// <param name="student">A hallgató, akin az ellenőrzés elvégzendő.</param>
        /// <param name="has26Exemption">Igaz, ha a hallgató mentességet élvez egy tantárgy teljesítése alól.</param>
        /// <param name="uses26Exemption">Igazra állítja a függvény, ha a hallgató felhasználja a mentességet.</param>
        private void ProcessCompulsoryCourses(Student student, bool has26Exemption, ref bool uses26Exemption)
        {
            // Kurzuskritériumok feldolgozása. Ezek a CRD kritériumfájlban foglalt kötelező kurzusok.
            // A végén a student.Result tagváltozó fog értékkel rendelkezni.
            // Ez azt ellenőrzi, hogy az első két szemeszter tárgyai megvannak-e.
            if (CourseCriteria != null)
            {
                // Kötelező tárgyak ellenőrzése az első két félévből.
                // TODO még egyszer ellenőrizni, hogy ez a függvény nem csinál hülyeséget
                base.ProcessCourseRequirements(student);

                // Mintatanterv szerinti kötelezően választhatók ellenőrzése
                // Teljesített kötelezően választhatók leszűrése
                var CompHumCourses = FilterCriteriaCourses(student, CompHumCourseGroup);
                // Összehasonlítás az előírt minimummal szakonként
                var CompHumResult = new Result(CompHumCourseGroup, GetCompHumAmount() <= CompHumCourses.Count());
                // Súly kiszámítása (súly == hiányzó kritériumok száma)
                if (CompHumResult)
                    CompHumResult.Weight = 0;
                else
                    CompHumResult.Weight = GetCompHumAmount() - CompHumCourses.Count();
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
                        uses26Exemption = true;
                        student.Result.Weight -= 1;
                        if (student.Result.Weight <= 0) // Ha a mentességnek
                        {
                            student.Result.Value = true;
                        }
                        Log.Write("Info: A mentességgel teljesül a követelmény: " + (student.Result.Value ? "igen" : "nem"));
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
        /// Megadja a mintatanterv elején figyelembevételre előírt kötelezően választható tárgymennyiséget.
        /// </summary>
        /// <returns>A mintatanterv elején előírt kötelezően választhatók száma</returns>
        private int GetCompHumAmount()
        {
            return SummaCriteria.FirstOrDefault(gr => gr.Identifier == CompHumCourseGroup).Amount;
        }
        
        /// <summary>
        /// A harmadik félévre vonatkozó követelmény feldolgozása. Ez 20 kredit, de 5 alól tankörivel felmentés kapható.
        /// </summary>
        /// <param name="student">A hallgató, akin az ellenőrzés elvégzendő.</param>
        private void ProcessSemester3Requirements(Student student, bool has26Exemption, ref bool uses26Exemption)
        {
            var semester3Courses = FilterCriteriaCourses(student, Semester3CourseGroup);
            var credits = (from c in semester3Courses
                           select c.Credit)
                           .Sum();
            // Követelmény ellenőrzése: legalább 20 kredit
            var result = new Result("Harmadik félévre vonatközó kritérium", 20 <= (int)credits);

            if (!result)
            {
                Log.Write("Info: A hallgatónak nincs 20 kreditje a harmadik félév mintatantervi tárgyaiból.");
                result.Weight = 20 - (int)credits; // Hiányzó kreditek száma
                if (has26Exemption && !uses26Exemption) // Mentesség
                {
                    uses26Exemption = true;
                    credits += 5;
                    result.Weight -= 5;
                    if (result.Weight <= 0) // Csak akkor, ha öt vagy kevesebb kredit hiányzott.
                    {
                        result.Weight = 0;
                        result.Value = true;
                    }
                    Log.Write("Info: A mentességgel teljesül a követelmény: " + (student.Result.Value ? "igen" : "nem"));
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
            // A megkövetelt mennyiség a workflow definition (.wd) fájlban van rögzítve.
            // Ha abban nem szerepel, akkor a követelmény 0.
            var result = new Result("Szigorlat",
                SummaCriteria.FirstOrDefault(gr => gr.Identifier == ExamGroup).Amount <= FilterCriteriaCourses(student, ExamGroup).Count());
        }

        /// <summary>
        /// Meghatározza, hogy a hallgatónak jár-e a 2. § (6) alapján (két teljesített tankörivel) a mentesség az (5) b) vagy c) alól.
        /// </summary>
        /// <param name="student"></param>
        /// <returns>Igaz, ha jár a mentesség.</returns>
        private bool Determine26Exemption(Student student)
        {
            return 2 <= FilterCriteriaCourses(student, StudyGroupGroup).Count();
        }

        /// <summary>
        /// Hozzáadja a hallgató jelentkezési listájához a specializációkat, amelyekre nem jelentkezett.
        /// </summary>
        /// <param name="student">A hallgató, akin a művelet végrehajtható.</param>
        private void AddMissingSpecializations(Student student, IEnumerable<SpecializationGrouping> specializationGroupings)
        {
            // Szűrés képzés alapján.
            var allValidSpecGroups = from sg in specializationGroupings
                                     where sg.EducationProgram == student.EducationProgram
                                     select sg;
            // Konkrét specializációk, amikre nem jelentkezett a hallgató (fix kapacitássorrendben)
            var missingSpecs = allValidSpecGroups.SelectMany(specGroup => specGroup)
                                                    .OrderBy(spec => spec.Capacity)
                                                    .Select(spec => spec.Name)
                                                    .Except(student.Choices);
            // Jelentkezési sorrend kiegészítése
            student.Choices = student.Choices.Concat(missingSpecs).ToArray();
            Log.Write(string.Format("Info: {0} specializációval egészítettük ki a hallgató jelentkezési sorrendjét.", missingSpecs.Count()));
        }

        /// <summary>
        /// A hallgató jelentkezési sorrendjéből eltávolítja azokat a specializációkat,
        /// amelyekre előkészítő tárgy hiánya miatt nem besorolható.
        /// </summary>
        /// <param name="student">A hallgató, akin a művelet végrehajtható</param>
        private void RemoveUnattainableSpecializations(Student student, IEnumerable<SpecializationGrouping> specializationGroupings)
        {
            List<string> validChoices = new List<string>(30);
            foreach (var specGroup in specializationGroupings.Where(sg => sg.EducationProgram == student.EducationProgram))
            {
                // Ha a specializációcsoporthoz nincs előírva előkészítő tárgycsoport vagy elő van írva és abból
                // legalább egy tantárgyat teljesített a hallgató, akkor lehetséges a besorolás a specializációkra.
                if (string.IsNullOrWhiteSpace(specGroup.PreSpecializationCourseGroup) ||
                    0 < FilterCriteriaCourses(student, PreSpecCourseGroup + specGroup.PreSpecializationCourseGroup).Count())
                {
                    validChoices.AddRange(from spec in specGroup select spec.Name);
                }
            }

            // Jelentkezési sor felülírása.
            student.Choices = student.Choices.Where(c => validChoices.Contains(c)).ToArray();
            Log.Write(string.Format("Info: {0} specializációra sorolható be a hallgató az előkészítő tárgyakat figyelembe véve.", validChoices.Count()));
        }

        /// <summary>
        /// A hallgató rangsorátlagának kiszámítása a szabályzat 3. §-a alapján.
        /// </summary>
        /// <param name="student">A hallgató, akinek a rangsorátlagát számítjuk.</param>
        private void CalculateRank(Student student, IEnumerable<SpecializationGrouping> specializationGroupings)
        {
            var semester12Courses = FilterCriteriaCourses(student, Semester12CourseGroup);
            var semester3Courses = FilterCriteriaCourses(student, Semester3CourseGroup);
            var semester4Courses = FilterCriteriaCourses(student, Semester4CourseGroup);
            // Kötelezően választhatóból a mintatanterv elejére ütemezett x darab (legjobb) figyelembe vétele
            var compHumCourses = FilterCriteriaCourses(student, CompHumCourseGroup).Take(GetCompHumAmount());
            // Előkészítő tárgyakból a három legjobb figyelembevétele
            var preSpecCourses = (from specGroup in specializationGroupings // Ez a szépség a teljesített specializációelőkészítő tárgyakat szedi össze
                                  select FilterCriteriaCourses(student, PreSpecCourseGroup + specGroup.PreSpecializationCourseGroup))
                                  .SelectMany(courses => courses)
                                  .Take(3);

            var allCourses =
                       (semester12Courses)
                .Concat(semester3Courses)
                .Concat(semester4Courses)
                .Concat(compHumCourses)
                .Concat(preSpecCourses);

            // A rangsorátlag számlálója
            // TODO átgondolni, hogy lehet-e olyan helyettesítés, hogy ne 120 kredittel osszunk
            student.Result.Points = (from c in allCourses
                                     select (c.Credit * c.Grade))
                                    .Sum();

            // Fixen 120 kredittel osztunk és egy körben végzünk besorolást.
            student.Result.Credit = 120;
            student.Round = 1;
        }
    }
}
