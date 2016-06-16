using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cornelius.Data;
using System.Globalization;

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
        /// Kötelezően választható (compulsory humanities) tárgyak csoportneve.
        /// </summary>
        private const string CompHumCourseSemester4Group = "KötelezőenVálasztható4Félév";

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
        /// <see cref="Evaluator.ProcessStudents"/> függvény.
        /// </summary>
        /// <param name="student">A hallgató, akinek a kritériummegfelelőségét ellenőrizni kell.</param>
        /// <param name="exception">Igaz, ha a hallgató fel van mentve a kritériumok alól.</param>
        /// <param name="specializationGroupings">A rendelkezésre álló specializációcsoportok.</param>
        public override void ProcessStudent(Student student, bool exception, IEnumerable<SpecializationGrouping> specializationGroupings)
        {
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
            Log.LeaveBlock();
        }

        /// <summary>
        /// Ezt a függvényt hívja meg a <see cref="ProcessStudent(Student, bool, IEnumerable{SpecializationGrouping})"/>
        /// a kritériumellenőrzéshez. Ellenőrzi, hogy a szabályzat 2. § (5) a-d) szerinti követelmények a
        /// 2. § (6) szerinti mentesség figyelembe vételével teljesülnek-e.
        /// </summary>
        /// <param name="student">A hallgató, akinek a kritériumait ellenőrizzük.</param>
        /// <param name="specializationGroupings">A rendelkezésre álló specializációcsoportok.</param>
        protected override void ProcessFinalResult(Student student, IEnumerable<SpecializationGrouping> specializationGroupings)
        {
            // 2. § (6) feldolgozása
            // Tanköri alapján mentesség meghatározása
            bool Has26Exemption = Determine26Exemption(student);
            bool Uses26Exemption = false;
            Log.Write("A hallgató rendelkezik engedményre jogosító két tankörivel: " + (Has26Exemption ? "igen" : "nem"));
            // Kreditek logolása tárgycsoportonként.
            LogCreditsByGroups(student, specializationGroupings);
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
            ProcessSpecializationRequirements(student);
            // Tanköri engedmény külön rögzítése.
            student.Result += new Result("Tanköri foglalkozás", Has26Exemption);
            Log.Write((student.Result ? "Teljesíti" : "Nem teljesíti") + " a jelentkezési kritériumokat a hallgató.");
        }

        private void LogCreditsByGroups(Student student, IEnumerable<SpecializationGrouping> specializationGroupings)
        {
            var semester12Courses = FilterCriteriaCourses(student, Semester12CourseGroup);
            var semester3Courses = FilterCriteriaCourses(student, Semester3CourseGroup);
            var semester4Courses = FilterCriteriaCourses(student, Semester4CourseGroup);
            var semester5xCourses = FilterCriteriaCourses(student, Semester5xCourseGroup);
            var preSpecCourses = (from specGroup in specializationGroupings // Ez a szépség a teljesített specializációelőkészítő tárgyakat szedi össze
                                  select FilterCriteriaCourses(student, PreSpecCourseGroup + specGroup.PreSpecializationCourseGroup))
                                  .SelectMany(courses => courses);
            var examCourses = FilterCriteriaCourses(student, ExamGroup);
            var compHumCourses =  FilterCriteriaCourses(student, CompHumCourseGroup);
            var freeChoiceCourses = FilterCriteriaCourses(student, FreeChoiceCourseGroup);
            student.CreditPerGroup = new Dictionary<string, double>();
            int allCredits = (int) (semester12Courses.Sum(c => c.Credit) + semester3Courses.Sum(c => c.Credit) + semester4Courses.Sum(c => c.Credit) + semester5xCourses.Sum(c => c.Credit) + preSpecCourses.Sum(c => c.Credit));
            student.CreditPerGroup.Add("Kötelező össz.", allCredits);
            student.CreditPerGroup.Add(RemoveDiacritics(Semester12CourseGroup), semester12Courses.Sum(c => c.Credit));
            student.CreditPerGroup.Add(RemoveDiacritics(Semester3CourseGroup), semester3Courses.Sum(c => c.Credit));
            student.CreditPerGroup.Add(RemoveDiacritics(Semester4CourseGroup), semester4Courses.Sum(c => c.Credit));
            student.CreditPerGroup.Add(RemoveDiacritics(Semester5xCourseGroup), semester5xCourses.Sum(c => c.Credit));
            student.CreditPerGroup.Add(RemoveDiacritics(PreSpecCourseGroup), preSpecCourses.Sum(c => c.Credit));
            student.CreditPerGroup.Add(RemoveDiacritics(ExamGroup) + "-OK", FilterCriteriaCourses(student, ExamGroup).Count() != 0 ? allCredits : 0);
            student.CreditPerGroup.Add(RemoveDiacritics(ExamGroup) + "-NINCS", FilterCriteriaCourses(student, ExamGroup).Count() == 0 ? allCredits : 0);
            int compHumCredits = compHumCourses.Sum(c => (int)c.Credit);
            student.CreditPerGroup.Add(RemoveDiacritics(CompHumCourseGroup), Math.Min(10, compHumCredits));
            int freeChoiceCredits = freeChoiceCourses.Sum(c => (int)c.Credit) + Math.Max(0, compHumCredits - 10);
            student.CreditPerGroup.Add(RemoveDiacritics(FreeChoiceCourseGroup), freeChoiceCredits);
        }

        private void ProcessSpecializationRequirements(Student student)
        {
            bool result = 0 < student.Choices.Length;
            if (!result)
            {
                student.Result.Value = false;
                student.Result += new Result("Specializációelőkészítők", false);
                Log.Write("A hallgató egyik specializációra sem sorolható be az előkészítők hiánya miatt.");
            }
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
            var credits = allCourses.Sum(course => course.Credit);

            // Kötválok és szabválok számítása max 10 kredit értékig.
            // TODO túlcsorduló kötvál kreditek számolása.
            int compHumCredits = (int)FilterCriteriaCourses(student, FreeChoiceCourseGroup).Sum(c => c.Credit);
            credits += Math.Min(10, compHumCredits);
            int freeChoiceCredits = (int)FilterCriteriaCourses(student, CompHumCourseGroup).Sum(c => c.Credit);
            credits += Math.Min(10, freeChoiceCredits + Math.Max(0, compHumCredits - 10));

            // Eredmény tárolása
            var result = new Result("Kreditkritérium", 90 <= (int)credits);
            if (!result)
            {
                result.Weight = 90 - (int)credits;
            }
            Log.Write("A kreditkritérium elfogadva: " + (result.Value ? "igen" : "nem"));

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
                base.ProcessCourseRequirements(student);

                // Mintatanterv szerinti kötelezően választhatók ellenőrzése
                // Teljesített kötelezően választhatók leszűrése
                var CompHumCourses = FilterCriteriaCourses(student, CompHumCourseGroup);
                // Összehasonlítás az előírt minimummal szakonként
                var CompHumResult = new Result(CompHumCourseGroup, GetCompHumAmount() <= CompHumCourses.Count());
                // Súly kiszámítása (súly == hiányzó kritériumok száma)
                CompHumResult.Weight = GetCompHumAmount();
                Log.Write(string.Format("Kötelezően választható tárgyak az első két félévből {0}.", CompHumResult.Value ? "teljesítve" : "nem teljesítve"));
                // Kötvál aleredmény tárolása
                student.Result.Weight += CompHumResult.Weight;
                student.Result += CompHumResult;
                student.Result.Value = student.Result.Value && CompHumResult.Value;

                // Ellenőrizzük, hogy a tanköri kedvezmény használható-e.
                if (student.Result.Value == false)
                {
                    //Log.Write("A hallgatónak nincs meg minden tantárgya az első két félévből.");
                    if (has26Exemption && !uses26Exemption)
                    {   // Kiváltható a kritérium
                        uses26Exemption = true;
                        student.Result.Weight += 1;
                        if (student.Result.Subresults.Sum(sr => sr.Weight) <= student.Result.Weight) // Ha a mentességnek
                        {
                            student.Result.Value = true;
                        }
                        Log.Write("A mentességgel teljesül a követelmény: " + (student.Result.Value ? "igen" : "nem"));
                    }
                }
                Log.Write("Első két félév kötelező tárgyai kritérium elfogadva: " + (student.Result.Value ? "igen" : "nem"));
            }
            else
            {
                Log.Write("Figyelmeztetés: A kurzuskritérium a kritériumellenőrzés során nincs beállítva.");
            }
        }

        /// <summary>
        /// Megadja a mintatanterv elején figyelembevételre előírt kötelezően választható tárgymennyiséget.
        /// </summary>
        /// <param name="all4">Igaz, ha az első négy félév kötelezően választható tárgyainak számát kérdezzük le</param>
        /// <returns>A mintatanterv elején előírt kötelezően választhatók száma</returns>
        private int GetCompHumAmount(bool all4 = false)
        {
            return SummaCriteria.FirstOrDefault(gr => gr.Identifier == (all4 ? CompHumCourseSemester4Group : CompHumCourseGroup)).Amount;
        }
        
        /// <summary>
        /// A harmadik félévre vonatkozó követelmény feldolgozása. Ez 20 kredit, de 5 alól tankörivel felmentés kapható.
        /// </summary>
        /// <param name="student">A hallgató, akin az ellenőrzés elvégzendő.</param>
        /// <param name="has26Exemption">Igaz, ha a hallgató a 2. § (6) (tanköri) alapján mentesülhet 5 kredit teljesítése alól.</param>
        /// <param name="uses26Exemption">Igaz, ha felhasználjuk a 2. § (6) alapján járó mentességet.</param>
        private void ProcessSemester3Requirements(Student student, bool has26Exemption, ref bool uses26Exemption)
        {
            var credits = FilterCriteriaCourses(student, Semester3CourseGroup).Sum(course => course.Credit);
            // Követelmény ellenőrzése: legalább 20 kredit
            var result = new Result("Harmadik félévre vonatközó kritérium", 20 <= (int)credits);

            if (!result)
            {
                Log.Write("A hallgatónak nincs 20 kreditje a harmadik félév mintatantervi tárgyaiból.");
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
                    Log.Write("A mentességgel teljesül a követelmény: " + (student.Result.Value ? "igen" : "nem"));
                }
            }
            Log.Write("A harmadik szemeszterből húsz kredit kritérium elfogadva: " + (result.Value ? "igen" : "nem"));

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
            Log.Write("Szigorlati kritérium elfogadva: " + (result.Value ? "igen" : "nem"));

            student.Result.Weight += result.Weight;
            student.Result += result;
            student.Result.Value = student.Result.Value && result.Value;
        }

        /// <summary>
        /// Meghatározza, hogy a hallgatónak jár-e a 2. § (6) alapján (két teljesített tankörivel) a mentesség az (5) b) vagy c) alól.
        /// </summary>
        /// <param name="student"></param>
        /// <returns>Igaz, ha jár a mentesség.</returns>
        private bool Determine26Exemption(Student student)
        {
            return SummaCriteria.FirstOrDefault(gr => gr.Identifier == StudyGroupGroup).Amount <= FilterCriteriaCourses(student, StudyGroupGroup).Count();
        }

        /// <summary>
        /// Hozzáadja a hallgató jelentkezési listájához a specializációkat, amelyekre nem jelentkezett.
        /// </summary>
        /// <param name="student">A hallgató, akin a művelet végrehajtható.</param>
        /// <param name="specializationGroupings">A rendelkezésre álló specializációcsoportok.</param>
        private void AddMissingSpecializations(Student student, IEnumerable<SpecializationGrouping> specializationGroupings)
        {
            // Szűrés képzés alapján.
            var allValidSpecGroups = specializationGroupings.Where(sg => sg.EducationProgram == student.EducationProgram);
            // Konkrét specializációk, amikre nem jelentkezett a hallgató (fix kapacitássorrendben)
            var missingSpecs = allValidSpecGroups.SelectMany(specGroup => specGroup)
                                                    .OrderBy(spec => spec.Capacity)
                                                    .Select(spec => spec.Name)
                                                    .Except(student.Choices);
            // Jelentkezési sorrend kiegészítése
            student.Choices = student.Choices.Concat(missingSpecs).ToArray();
            Log.Write(string.Format("{0} specializációval egészítettük ki a hallgató jelentkezési sorrendjét.", missingSpecs.Count()));
        }

        /// <summary>
        /// A hallgató jelentkezési sorrendjéből eltávolítja azokat a specializációkat,
        /// amelyekre előkészítő tárgy hiánya miatt nem besorolható.
        /// </summary>
        /// <param name="student">A hallgató, akin a művelet végrehajtható</param>
        /// <param name="specializationGroupings">A rendelkezésre álló specializációcsoportok.</param>
        private void RemoveUnattainableSpecializations(Student student, IEnumerable<SpecializationGrouping> specializationGroupings)
        {
            List<string> validChoices = new List<string>();
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
            student.Choices = student.Choices.Where(choice => validChoices.Contains(choice)).ToArray();
            Log.Write(string.Format("{0} specializációra sorolható be a hallgató az előkészítő tárgyakat figyelembe véve.", validChoices.Count()));
        }

        /// <summary>
        /// A hallgató rangsorátlagának kiszámítása a szabályzat 3. §-a alapján.
        /// </summary>
        /// <param name="student">A hallgató, akinek a rangsorátlagát számítjuk.</param>
        /// <param name="specializationGroupings">A rendelkezésre álló specializációcsoportok.</param>
        private void CalculateRank(Student student, IEnumerable<SpecializationGrouping> specializationGroupings)
        {
            var semester12Courses = FilterCriteriaCourses(student, Semester12CourseGroup);
            var semester3Courses = FilterCriteriaCourses(student, Semester3CourseGroup);
            var semester4Courses = FilterCriteriaCourses(student, Semester4CourseGroup);
            // Kötelezően választhatóból a mintatanterv elejére ütemezett x darab (legjobb) figyelembe vétele
            var compHumCourses = FilterCriteriaCourses(student, CompHumCourseGroup).Take(GetCompHumAmount(true));
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

            // A rangsorátlag számításához
            // TODO átgondolni, hogy lehet-e olyan helyettesítés, hogy ne 120 kredittel osszunk
            student.Result.Points = allCourses.Sum(course => course.Credit * course.Grade);
            student.Result.Credit = 120;
            student.Round = 1;

            Log.Write(string.Format("A hallgató rangsorátlaga {0}.", student.Result.Avarage));
        }

        public static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
