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
             *  - Lefuttatjuk a besorolási algoritmust (lásd lejjebb).
             *  - Lemezre mentjük az eredményeket.
             * 
             * A besorolási algoritmus:
             *  - A hallgatókat képzésenként besorolási sorrendben lekérdezzük.
             *  - A lista elején lévő besorolatlan hallgatót a számára legelőnyösebb specializációra besoroljuk.
             *      - A jelentkezési sorrendjében ellenőrzzük, hogy besorolható-e az adott specializációra (ágazatra). Ez a Létszámellenőrzés*.
             *      - Minimumkritérium nem teljesülés esetén a többi specializációcsoportot illetve ágazatot le kell zárni.
             *      - Ha besorolható, akkor besoroljuk.
             *      - Ha sehova nem sorolható be a jelentkezései közül:
             *          - Ha minden specilaizáció benne volt a jelentkezései közt, akkor senki nem sorolható már be, az algoritmus véget ér.
             *          - Ha nem minden specializáción volt jogosult besorolásra, akkor kritériumhiányosnak minősül és újraindul a besorolási algoritmus nélküle.
             *  - Minden besorolás után elvégezzük a létszámellenőrzést. (*)
             *      - Sikertelen létszámellenőrzés esetén az összes minimumlétszámot elérő specializációt lezárjuk.
             *  
             *  (*) Létszámellenőrzés: annak a feltételnek a teljesülését vizsgáljuk, hogy a dékáni utasításban megadott arányok és kapacitáskorlátok alapján
             *      az adott specializációcsoportra és ágazatra a hallgató a maximumkritériumokat betartva befér valamint a többi ágazaton a minimumkritériumok
             *      továbbra is teljesíthetőek maradnak.
            **/

            var import = Import.LoadAllFiles();
            var allStudents = Builder.ExtractStudents(import);
            var specializations = Builder.ExtractSpecializations(import);
            var specGroupings = Builder.ExtractSpecializationGroupings(import, specializations);
            Evaluator.ProcessStudents(allStudents, import.Exceptions, specGroupings);
            
            var studentsByEduProgram = allStudents
                .Where(student => student.Result)
                .OrderBy(student => student.Round)
                .ThenByDescending(student => student.Result.Avarage)
                .GroupBy(student => student.EducationProgram);
            var specGroupingsByEduProgram = specGroupings.GroupBy(sg => sg.EducationProgram);

            // Képzési programokon való iteráció
            foreach (var studentsInProgram in studentsByEduProgram)
            {
                bool successfulPlacement = false;
                int tries = 0;
                List<Student> studentList = new List<Student>(studentsInProgram);
                IEnumerable<Tuple<Student, Specialization>> placements = null;
                Student unplaceableStudent;

                // Besorolási kísérletek. Ha közben találunk kritériumhiányosnak találunk egy hallgatót, akkor a újra kell indítani a besorolást.
                while (!successfulPlacement && tries < studentsInProgram.Count())
                {
                    tries++;
                    Log.Write("Info: " + tries.ToString() + ". besorolási próbálkozás a " + studentsInProgram.Key + " képzésen.");
                    if (TryPlaceStudents(studentList, specGroupingsByEduProgram.First(g => g.Key == studentsInProgram.Key), out placements, out unplaceableStudent))
                    {
                        Log.Write("Info: Sikeresen lefutott a besorolás.");
                        successfulPlacement = true;
                        break;
                    }
                    else
                    {
                        Log.Write("Info: Sikertelen besorolás, egy kritériumhiányos hallgató (" + unplaceableStudent.OriginKey + ") kiszűrve.");
                        studentList.Remove(unplaceableStudent);
                        unplaceableStudent.Result.Value = false;
                        unplaceableStudent.Result += new Result("Rangsorolás során kritériumhiányos az összes lehetséges specializáción.", false);
                    }
                }
                if (successfulPlacement)
                {
                    // Sikeresen véget ért besorolás esetén az eredmények véglegesítése.
                    foreach (var tuple in placements)
                    {
                        var spec = tuple.Item2;
                        spec += tuple.Item1;
                    }
                    break;
                }
            }

            Log.Write("Eredmény lemezre írása...");
            Log.EnterBlock();
            if (!Directory.Exists(Program.OUTPUT_DIRECTORY))
            {
                Directory.CreateDirectory(Program.OUTPUT_DIRECTORY);
            }
            Export.ExportDatabases(allStudents);
            Export.ExportReports(allStudents, specializations);
            Export.ExportCreditStatistics(allStudents);
            Log.LeaveBlock();
        }

        /// <summary>
        /// Kísérletet tesz egy képzés hallgatóinak besorolására. Ha ez lehetséges, akkor visszaadja az eredményt, hogy az véglegesíthető legyen.
        /// Ha nem lehetséges, akkor visszaadja, hogy melyik hallgató kritériumhiánya miatt kell a besorolást újraindítani.
        /// </summary>
        /// <param name="students">A képzésen a besorolható hallgatók a rangsor szerinti sorrendben.</param>
        /// <param name="specializationGroups">A képzéshez tartozó specializációcsoportok.</param>
        /// <param name="placements">Sikeres besorolás esetén a besorolás eredménye (hallgató-specializáció összerendelés), egyébként null.</param>
        /// <param name="unplaceableStudent">Sikertelen besorolási kísérlet esetén a kritériumhiányosnak talált hallgató, egyébként null.</param>
        /// <returns>Igaz, ha sikeres a teljes besorolás, egyébként hamis. Ha csak egy hallgató esett ki, azt a <see cref="unplaceableStudent"/> paraméter tartalmazza.</returns>
        private static bool TryPlaceStudents(IEnumerable<Student> students, IEnumerable<SpecializationGrouping> specializationGroups, out IEnumerable<Tuple<Student, Specialization>> placements, out Student unplaceableStudent)
        {
            // Besorolható hallgatók száma.
            var totalHeadcount = students.Count();
            // A besorolási kísérlet során az alábbi két változóban tartjuk számon az érvényes minimumlétszámokat, maximumlétszámokat és az aktuálisan besorolt hallgatói számokat.
            // A specializációcsoportok létszámai (minimum, maximum, besorolás adott fázisában a tényleges).
            var specGroupHeadcounts = new Dictionary<SpecializationGrouping, Tuple<int, int, int>>();
            // A specializációk (ágazatok) létszámai (besorolás adott fázisában a minimum, besorolás adott fázisában a maximum, besorolás adott fázisában a tényleges).
            var specHeadcounts = new Dictionary<Specialization, Tuple<int, int, int>>();
            // A hallgató-specializáció összerendeléseket tartalmazó lista.
            var resultsList = new List<Tuple<Student, Specialization>>(totalHeadcount);
            // A specializációk listája.
            var specializations = specializationGroups.SelectMany(spec => spec);

            // Kezdő létszámok (min, max, aktuális) létrehozása.
            foreach (var specGroup in specializationGroups)
            {
                specGroupHeadcounts.Add(specGroup, Tuple.Create(specGroup.GetMinCount(totalHeadcount), specGroup.GetMaxCount(totalHeadcount), 0));
                foreach (var spec in specGroup)
                    specHeadcounts.Add(spec, Tuple.Create(GetSpecMinCount(spec, specGroup, 0, totalHeadcount), GetSpecMaxCount(spec, specGroup, 0, totalHeadcount, totalHeadcount), 0));
            }

            // A besorolóalgoritmus. Az első sikertelen besorolás esetén leáll.
            unplaceableStudent = null;
            foreach (var student in students)
            {
                // Preferenciasorrendben ellenőrizzük, hogy besorolható-e, és az első helyre besoroljuk.
                bool studentPlaced = false;
                foreach (var choice in student.Choices)
                {
                    var choiceSpec = specializations.First(spec => spec.Name == choice);
                    bool reasonSpecMin, reasonSpecGroupMin;
                    if (CanPlaceStudent(choiceSpec, specializationGroups, specHeadcounts, specGroupHeadcounts, totalHeadcount, out reasonSpecMin, out reasonSpecGroupMin))
                    {
                        PlaceStudent(choiceSpec, specializationGroups, specHeadcounts, specGroupHeadcounts, totalHeadcount);
                        resultsList.Add(Tuple.Create(student, choiceSpec));
                        studentPlaced = true;
                        break;
                    }
                    // Minimumkritérium nemteljesítése esetén a többi specializációt (ágazatot) illetve specializációcsoportot le kell zárni a lehető legalacsonyabb létszámnál.
                    // Lezárás: ha a minimumlétszámot még nem értük el, akkor az új maximumlétszám a minimumlétszám értéke lesz, egyébként pedig az aktuális létszám.
                    // Ha az ágazaton nem teljesíthető a besorolás esetén a minimum, akkor csak a specializációcsoport ágazatait zárjuk le.
                    if (reasonSpecMin)
                        foreach (var spec in specializationGroups.First(sg => sg.Contains(choiceSpec)))
                            specHeadcounts[spec] =             Tuple.Create(specHeadcounts[spec].Item1,
                                                                            Math.Max(specHeadcounts[spec].Item1, specHeadcounts[spec].Item3),
                                                                            specHeadcounts[spec].Item3);
                    // Ha a specializációcsoportok között nem teljesíthető a minimumkritérium, akkor az összes specializációcsoportot zárjuk le, de az ágazatokat nem.
                    if (reasonSpecGroupMin)
                        foreach (var specGroup in specializationGroups)
                            specGroupHeadcounts[specGroup] =   Tuple.Create(specGroupHeadcounts[specGroup].Item1,
                                                                            Math.Max(specGroupHeadcounts[specGroup].Item1, specGroupHeadcounts[specGroup].Item3),
                                                                            specGroupHeadcounts[specGroup].Item3);
                }
                if (!studentPlaced)
                {
                    // Ha nem mindenhova próbáltuk besorolni a hallgatót, akkor ő számít kritériumhiányosnak.
                    if (specializations.Count() != student.Choices.Count())
                        unplaceableStudent = student;
                    break;
                }
            }

            // Ha nincs konkrét kritériumhiányos hallgató, akkor több hallgató egyáltalán nem sorolható be
            placements =    (unplaceableStudent == null) ? resultsList  : null;
            return          (unplaceableStudent == null) ? true         : false;
        }

        /// <summary>
        /// Hallgató beleszámolása egy adott specializáció (ágazat) és az azt bennfoglaló specializációcsoport létszámába.
        /// </summary>
        /// <param name="specialization">A specializáció (ágazat), ahova a hallgatót soroljuk.</param>
        /// <param name="specializationGroups">A specializációcsoportok listája.</param>
        /// <param name="specHeadcounts">A specializációk (ágazatok) létszámai (min, max, aktuális).</param>
        /// <param name="specGroupHeadcounts">A specializációcsoportok létszámai (min, max, aktuális).</param>
        /// <param name="totalHeadcount">A teljes besorolható létszám.</param>
        private static void PlaceStudent(
            Specialization specialization,
            IEnumerable<SpecializationGrouping> specializationGroups,
            IDictionary<Specialization, Tuple<int, int, int>> specHeadcounts,
            IDictionary<SpecializationGrouping, Tuple<int, int, int>> specGroupHeadcounts,
            int totalHeadcount)
        {
            var currSpecGroup = specializationGroups.First(sg => sg.Contains(specialization));
            var currSpecGroupHc = specGroupHeadcounts[currSpecGroup];

            // Specializációcsoport létszámának frissítése
            specGroupHeadcounts[currSpecGroup] = Tuple.Create(  currSpecGroupHc.Item1,
                                                                currSpecGroupHc.Item2,
                                                                currSpecGroupHc.Item3 + 1);
            // Az egyes ágazatokon dinamikusan változik a specializációcsoportba besorolt hallgatók függvényében, hogy mik a minimumok és maximumok.
            foreach (var spec in currSpecGroup)
                specHeadcounts[spec] = Tuple.Create(
                    GetSpecMinCount(spec, currSpecGroup, currSpecGroupHc.Item3, totalHeadcount),
                    GetSpecMaxCount(spec, currSpecGroup, currSpecGroupHc.Item3, totalHeadcount, totalHeadcount - specGroupHeadcounts.Sum(kvSpecGroup => kvSpecGroup.Value.Item3)),
                    specHeadcounts[specialization].Item3 + (spec == specialization ? 1 : 0));
        }

        /// <summary>
        /// Meghatározza, hogy egy adott specializációra besorolható-e a hallgató a létszámok alapján (egy esetben sem haladja meg a létszám a maximumot és a minimumok teljesíthetőek).
        /// </summary>
        /// <param name="specialization">A specializáció (ágazat), ahova a hallgatót soroljuk.</param>
        /// <param name="specializationGroups">A specializációcsoportok listája.</param>
        /// <param name="specHeadcounts">A specializációk (ágazatok) létszámai (min, max, aktuális).</param>
        /// <param name="specGroupHeadcounts">A specializációcsoportok létszámai (min, max, aktuális).</param>
        /// <param name="totalHeadcount">A teljes besorolható létszám.</param>
        /// <param name="specializationMinimum">Igaz, ha a besorolással a specializációcsoporton belül egy specializáción a minimumkritérium nem lenne teljesíthető.</param>
        /// <param name="specializationGroupMinimum">Igaz, ha a besorolással egy specializációcsoportban a minimumkritérium nem lenne teljesíthető.</param>
        /// <returns>Igazat ad vissza, ha a létszámokat figyelembe véve a hallgató besorolható, és hamisat, ha valamely okból nem.</returns>
        private static bool CanPlaceStudent(
            Specialization specialization,
            IEnumerable<SpecializationGrouping> specializationGroups,
            IDictionary<Specialization, Tuple<int, int, int>> specHeadcounts,
            IDictionary<SpecializationGrouping, Tuple<int, int, int>> specGroupHeadcounts,
            int totalHeadcount,
            out bool specializationMinimum,
            out bool specializationGroupMinimum)
        {
            var currSpecGroup = specializationGroups.First(sg => sg.Contains(specialization));
            var unplacedCount = totalHeadcount - specGroupHeadcounts.Sum(kvSpecGroup => kvSpecGroup.Value.Item3);
            bool result;

            // Ágazatra nem fér be, ha [max létszám <= aktuális]
            var specializationMaximum = specHeadcounts[specialization].Item2 <= specHeadcounts[specialization].Item3;
            // Specializációcsoportra nem fér be, ha [max létszám <= aktuális]
            var specializationGroupMaximum = specGroupHeadcounts[currSpecGroup].Item2 <= specGroupHeadcounts[currSpecGroup].Item3;
            // Specializációcsoportoknál a minimum akkor nem teljesíthető, ha [be nem sorolt hallgatók száma < a minimumra töltéshez szükséges létszám]
            // Figyelembe véve azt az esetet is, ha már minimumlétszám felett van egy specializáció és azt is, ha épp minimumlétszám alattira sorolnánk be a hallgatót.
            specializationGroupMinimum = unplacedCount < specializationGroups.Sum(sg => Math.Max(0, specGroupHeadcounts[sg].Item3 - specGroupHeadcounts[sg].Item1 - (sg == currSpecGroup ? 1 : 0)));
            // Ágazatoknál a minimum akkor nem teljesíthető, ha [ágazatokra még besorolható hallgatók száma < ágazatok minimumra töltéséhez szükséges létszám]
            //  - [Ágazatokra még besorolható hallgatók száma == be nem sorolt hallgatók száma - többi specializációcsoport minimumra töltéséhez szükséges létszám]
            //  - Ágazat minimumra töltéséhez szükséges létszámnál figyelembe véve, hogy melyikre sorolnánk éppen a hallgatót
            specializationMinimum = unplacedCount - specializationGroups.Where(sg => sg != currSpecGroup).Sum(sg => Math.Max(0, specGroupHeadcounts[sg].Item3 - specGroupHeadcounts[sg].Item1))
                < currSpecGroup.Sum(spec => Math.Max(0, specHeadcounts[spec].Item3 - specHeadcounts[spec].Item1 - (spec == specialization ? 1 : 0)));

            result = specializationMinimum && specializationGroupMinimum && specializationMaximum && specializationGroupMaximum;
            Log.Write(string.Format("Info: A {0} specializációra (ágazatra) besorolási kísérlet lehetséges: ", specialization.Name, result ? "igen" : "nem"));
            Log.Write(string.Format("Kritériumhiány az utóbbiaknál - specializációcsoport (min: {0}, max: {1}), ágazat (min: {2}, max: {3})", specializationGroupMinimum, specializationGroupMaximum, specializationMinimum, specializationMaximum));
            return result;
        }

        /// <summary>
        /// Becsli egy specializációra (ágazatra) besorolandó minimális létszámát a specializációcsoport besorolás közbeni aktuális létszáma alapján.
        /// </summary>
        /// <param name="spec">A specializáció (ágazat).</param>
        /// <param name="specGroup">A specializációcsoport, ahova az ágazat tartozik.</param>
        /// <param name="specGroupCount">A specializációcsoport aktuális létszáma.</param>
        /// <param name="totalCount">A teljes besorolható létszám.</param>
        /// <returns>A specializációra (ágazatra) besorolandó minimális létszámra adott becslés.</returns>
        private static int GetSpecMinCount(Specialization spec, SpecializationGrouping specGroup, int specGroupCount, int totalCount)
        {
            // A minimális specializációcsoport-létszám alapján becsüljük a minimumot. A tényleges minimum lehet nagyobb.
            int minSpecGroupCount = Math.Max(specGroup.GetMinCount(totalCount), specGroupCount);
            return spec.GetMinCount(minSpecGroupCount);
        }

        /// <summary>
        /// Becsli egy specializációra (ágazatra) besorolható maximális létszámát a specializációcsoport besorolás közbeni aktuális létszáma alapján.
        /// </summary>
        /// <param name="spec">A specializáció (ágazat).</param>
        /// <param name="specGroup">A specializációcsoport, ahova az ágazat tartozik.</param>
        /// <param name="specGroupCount">A specializációcsoport aktuális létszáma.</param>
        /// <param name="totalCount">A teljes besorolható létszám.</param>
        /// <param name="unplacedCount">A még be nem sorolt hallgatók létszáma.</param>
        /// <returns>A specializációra (ágazatra) besorolható maximális létszámra adott becslés.</returns>
        private static int GetSpecMaxCount(Specialization spec, SpecializationGrouping specGroup, int specGroupCount, int totalCount, int unplacedCount)
        {
            // A maximális specializációcsoport-létszám alapján becsüljük a maximumot. A tényleges maximum lehet kisebb.
            int maxSpecGroupCount = Math.Min(specGroup.GetMaxCount(totalCount), specGroupCount + unplacedCount);
            return spec.GetMaxCount(maxSpecGroupCount);
        }
    }
}
