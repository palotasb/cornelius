using Cornelius.Criteria;
using Cornelius.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cornelius.Placement
{
    class Algorithm
    {
        /* A besorolási algoritmus:
         *  - A hallgatókat képzésenként besorolási sorrendben lekérdezzük.
         *  - A lista elején lévő besorolatlan hallgatót a számára legelőnyösebb specializációra besoroljuk.
         *      - A jelentkezési sorrendjében ellenőrizzük, hogy besorolható-e az adott specializációra (ágazatra). Ez a Létszámellenőrzés*.
         *      - Minimumkritérium nem teljesülése esetén a többi specializációcsoportot illetve ágazatot le kell zárni.
         *      - Maximumkritérium nem teljesülése esetén, ha a hallgató azonos átlaggal rendelkezik már bekerült hallgatóval, akkor a többi azonos átlagú
         *      hallgatót is kivesszük, és úgy indítjuk újra a besorolást, hogy épp előttük érjük el a kapacitáshatárt.
         *          - Megjegyzés: ezzel minimumkritériumot sérthetünk.
         *          TODO alternatív megoldásként megfontolandó, hogy esetleg minimum és maximumkritériumokat is megsértve besoroljuk az összes hallgatót.
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

        /// <summary>
        /// Az algoritmusban a feldolgozandó összes besorolhat hallgató listája preferenciasorrendben.
        /// </summary>
        private IEnumerable<Student> Students;

        /// <summary>
        /// Az algoritmusban a felhasználható összes specializációcsoport.
        /// </summary>
        private IEnumerable<SpecializationGrouping> SpecializationGroupings;

        /// <summary>
        /// Az algoritmusban a felhasználható összes specializáció (ágazat).
        /// </summary>
        private IEnumerable<Specialization> Specializations;

        /// <summary>
        /// Új besorolóalgoritmus létrehozása rangsorátlaggal rendelkező hallgatók és specializációcsoportok alapján.
        /// </summary>
        /// <param name="students">A képzésen a besorolandó hallgatók.</param>
        /// <param name="specializationGroupings">A képzéshez tartozó specializációcsoportok.</param>
        public Algorithm(IEnumerable<Student> students, IEnumerable<SpecializationGrouping> specializationGroupings)
        {
            Students = from student in students where student.Result orderby student.Round, student.Result.Avarage descending select student;
            SpecializationGroupings = specializationGroupings;
            Specializations = SpecializationGroupings.SelectMany(spec => spec);
        }

        /// <summary>
        /// A besorolás elkészítése és a siker visszajelzése.
        /// </summary>
        /// <param name="placements">Sikeres besorolás esetén a hallgató-specializáció párosítások, egyébként null.</param>
        /// <returns>Igaz, ha a besorolás sikeres.</returns>
        public bool Run(out IEnumerable<Tuple<Student, Specialization>> placements)
        {
            int tries = 0;
            List<Student> studentList = new List<Student>(Students);
            Student unplaceableStudent;
            placements = null;

            // Besorolási kísérletek. Ha közben találunk kritériumhiányosnak találunk egy hallgatót, akkor a újra kell indítani a besorolást.
            var specLimits = new Dictionary<Specialization, int>();
            bool restart = true;
            while (restart)
            {
                Log.Write("Info: " + tries.ToString() + ". besorolási próbálkozás a képzésen.");
                Log.EnterBlock();
                if (TryPlaceStudents(studentList, specLimits, out placements, out unplaceableStudent, out restart))
                {
                    Log.Write("Info: Sikeresen lefutott a besorolás.");
                    return true;
                }
                Log.Write("Info: Sikertelen besorolás, egy kritériumhiányos hallgató (" + unplaceableStudent.OriginKey + ") kiszűrve.");
                if (unplaceableStudent != null)
                {
                    Log.Write("Info: Kritériumhiányos hallgató (" + unplaceableStudent.OriginKey + ") kiszűrve.");
                    studentList.Remove(unplaceableStudent);
                    unplaceableStudent.Result.Value = false;
                    unplaceableStudent.Result += new Result("Rangsorolás során kritériumhiányos az összes lehetséges specializáción.", false);
                }
                Log.LeaveBlock();
            }
            return false;
        }

        /// <summary>
        /// Kísérletet tesz egy képzés hallgatóinak besorolására. Ha ez lehetséges, akkor visszaadja az eredményt, hogy az véglegesíthető legyen.
        /// Ha nem lehetséges, akkor visszaadja, hogy melyik hallgató kritériumhiánya miatt kell a besorolást újraindítani.
        /// </summary>
        /// <param name="placeableStudents">A besorolható hallgatók listája.</param>
        /// <param name="specLimits">További felső korlátok az egyes specializációk létszámára.</param>
        /// <param name="placements">Sikeres besorolás esetén a besorolás eredménye (hallgató-specializáció összerendelés), egyébként null.</param>
        /// <param name="unplaceableStudent">Sikertelen besorolási kísérlet esetén a kritériumhiányosnak talált hallgató, egyébként null.</param>
        /// <param name="restart">Igaz, ha a besorolás újraindítandó a frissített paraméterekkel.</param>
        /// <returns>Igaz, ha sikeres a teljes besorolás, egyébként hamis. Ha csak egy hallgató esett ki, azt a <see cref="unplaceableStudent"/> paraméter tartalmazza.</returns>
        private bool TryPlaceStudents(
            IEnumerable<Student> placeableStudents,
            IDictionary<Specialization, int> specLimits,
            out IEnumerable<Tuple<Student, Specialization>> placements,
            out Student unplaceableStudent,
            out bool restart)
        {
            // A besorolási kísérlet során az alábbi két változóban tartjuk számon az érvényes minimumlétszámokat, maximumlétszámokat és az aktuálisan besorolt hallgatói számokat.
            // A specializációcsoportok létszámai (minimum, maximum, besorolás adott fázisában a tényleges).
            var specGroupHeadcounts = new Dictionary<SpecializationGrouping, SpecializationGroupingHeadcount>();
            // A specializációk (ágazatok) létszámai (besorolás adott fázisában a minimum, maximum, tényleges létszám, magasabb átlagú hallgatók száma, minimumátlag).
            var specHeadcounts = new Dictionary<Specialization, SpecializationHeadcount>();
            // Besorolható hallgatók száma.
            var totalHeadcount = placeableStudents.Count();
            foreach (var specGroup in SpecializationGroupings)
            {
                specGroupHeadcounts.Add(specGroup, new SpecializationGroupingHeadcount(specGroup, totalHeadcount));
                foreach (var spec in specGroup)
                {
                    specHeadcounts.Add(spec, new SpecializationHeadcount(spec, specGroup, totalHeadcount));
                    if (specLimits.ContainsKey(spec))
                        specHeadcounts[spec].CloseAt(specLimits[spec]);
                }
            }

            // A besorolóalgoritmus. Az első sikertelen besorolás esetén leáll, és jelzi, ha újra kell indítani.
            // A hallgató-specializáció összerendeléseket tartalmazó lista.
            var resultsList = new List<Tuple<Student, Specialization>>(totalHeadcount);
            unplaceableStudent = null;
            restart = false;
            foreach (var student in placeableStudents)
            {
                // Preferenciasorrendben ellenőrizzük, hogy besorolható-e, és az első helyre besoroljuk.
                bool studentPlaced = false;
                foreach (var choice in student.Choices)
                {
                    var choiceSpec = Specializations.First(spec => spec.Name == choice);
                    bool reasonSpecMin, reasonSpecGroupMin, reasonMaxSameAvg;
                    if (CanPlaceStudent(choiceSpec, specHeadcounts, specGroupHeadcounts, totalHeadcount, student.Result.Avarage, out reasonSpecMin, out reasonSpecGroupMin, out reasonMaxSameAvg))
                    {   // Létszámok stimmelnek, a hallgató besorolható.
                        UpdateHeadcountsWithPlacement(choiceSpec, specHeadcounts, specGroupHeadcounts, totalHeadcount, student.Result.Avarage);
                        resultsList.Add(Tuple.Create(student, choiceSpec));
                        studentPlaced = true;
                        break; // choice in student.Choices
                    }
                    if (reasonSpecMin)  // Ha az ágazaton nem teljesíthető a besorolás esetén a minimum, akkor csak a specializációcsoport ágazatait zárjuk le.
                        foreach (var spec in SpecializationGroupings.First(sg => sg.Contains(choiceSpec)))
                            specHeadcounts[spec].Close();
                    if (reasonSpecGroupMin) // Ha a specializációcsoportok között nem teljesíthető a minimumkritérium, akkor az összes specializációcsoportot zárjuk le, de az ágazatokat nem.
                        foreach (var specGroup in SpecializationGroupings)
                            specGroupHeadcounts[specGroup].Close();
                    if (reasonMaxSameAvg)
                    {   // Ha a maximumkritérium úgy nem teljesíthető, hogy a hallgató az utolsó besorolt hallgatóval azonos átlagú
                        specLimits.Add(choiceSpec, specHeadcounts[choiceSpec].AboveMinimumCount);
                        placements = null;
                        restart = true;
                        return false;
                    }
                }
                if (!studentPlaced)
                {
                    if (Specializations.Count() != student.Choices.Count())
                    {   // Ha nem mindenhova próbáltuk besorolni a hallgatót, akkor ő számít kritériumhiányosnak.
                        unplaceableStudent = student;
                        restart = true;
                    }
                    break;  // student in placeableStudent :: A besorolás itt mindenképp véget ért, de kritériumhiányos hallgató esetén újraindítjuk.
                }
            }

            // Ha nincs konkrét kritériumhiányos hallgató, akkor több hallgató egyáltalán nem sorolható be és sikerrel zártuk az algoritmust.
            placements =    (unplaceableStudent == null) ? resultsList : null;
            restart =       (unplaceableStudent != null);
            return          (unplaceableStudent == null);
        }

        /// <summary>
        /// Hallgató beleszámolása egy adott specializáció (ágazat) és az azt bennfoglaló specializációcsoport létszámába.
        /// </summary>
        /// <param name="specialization">A specializáció (ágazat), ahova a hallgatót soroljuk.</param>
        /// <param name="specHeadcounts">A specializációk (ágazatok) létszámai (min, max, aktuális).</param>
        /// <param name="specGroupHeadcounts">A specializációcsoportok létszámai (min, max, aktuális).</param>
        /// <param name="totalHeadcount">A teljes besorolható létszám.</param>
        /// <param name="rankAverage">A besorolandó hallgató rangsorátlaga.</param>
        private void UpdateHeadcountsWithPlacement(
            Specialization specialization,
            IDictionary<Specialization, SpecializationHeadcount> specHeadcounts,
            IDictionary<SpecializationGrouping, SpecializationGroupingHeadcount> specGroupHeadcounts,
            int totalHeadcount,
            double rankAverage)
        {
            var currSpecGroup = SpecializationGroupings.First(sg => sg.Contains(specialization));
            specGroupHeadcounts[currSpecGroup].CurrentCount++;
            // A specializációcsoport összes minimuma és maximuma változhat.
            foreach (var spec in currSpecGroup)
            {
                if (!specHeadcounts[spec].Closed)
                {   // Lezárás után már a minimumok és maximumok nem változhatnak, és itt nem szabad, hogy felülírjuk őket.
                    specHeadcounts[spec].MinimumCount = GetSpecMinCount(spec, currSpecGroup, specGroupHeadcounts[currSpecGroup].CurrentCount, totalHeadcount);
                    specHeadcounts[spec].MaximumCount = GetSpecMaxCount(spec, currSpecGroup, specGroupHeadcounts[currSpecGroup].CurrentCount, totalHeadcount, totalHeadcount - specGroupHeadcounts.Sum(kvSpecGroup => kvSpecGroup.Value.CurrentCount));
                }
                if (spec == specialization)
                {   // Ha erre a specializációra (ágazatra) soroltunk éppen be, akkor frissítjük a számokat.
                    specHeadcounts[spec].CurrentCount++;
                    specHeadcounts[spec].AboveMinimumCount += (rankAverage < specHeadcounts[spec].MinimumRankAverage ? 1 : 0);
                    specHeadcounts[spec].MinimumRankAverage = rankAverage;
                }
            }
        }

        /// <summary>
        /// Meghatározza, hogy egy adott specializációra besorolható-e a hallgató a létszámok alapján (egy esetben sem haladja meg a létszám a maximumot és a minimumok teljesíthetőek).
        /// </summary>
        /// <param name="specialization">A specializáció (ágazat), ahova a hallgatót soroljuk.</param>
        /// <param name="specHeadcounts">A specializációk (ágazatok) létszámai (min, max, aktuális).</param>
        /// <param name="specGroupHeadcounts">A specializációcsoportok létszámai (min, max, aktuális).</param>
        /// <param name="totalHeadcount">A teljes besorolható létszám.</param>
        /// <param name="rankAverage">A besorolandó hallgató rangsorátlaga.</param>
        /// <param name="specializationMinimum">Igaz, ha a besorolással a specializációcsoporton belül egy specializáción a minimumkritérium nem lenne teljesíthető.</param>
        /// <param name="specializationGroupMinimum">Igaz, ha a besorolással egy specializációcsoportban a minimumkritérium nem lenne teljesíthető.</param>
        /// <param name="specializationMaximumAndSameRank">Igaz, ha a hallgató a specializácóra a hallgató nem fér be, de a rangsorátlaga nem kisebb minden bekerült hallgatónál.</param>
        /// <returns>Igazat ad vissza, ha a létszámokat figyelembe véve a hallgató besorolható, és hamisat, ha valamely okból nem.</returns>
        private bool CanPlaceStudent(
            Specialization specialization,
            IDictionary<Specialization, SpecializationHeadcount> specHeadcounts,
            IDictionary<SpecializationGrouping, SpecializationGroupingHeadcount> specGroupHeadcounts,
            int totalHeadcount,
            double rankAverage,
            out bool specializationMinimum,
            out bool specializationGroupMinimum,
            out bool specializationMaximumAndSameRank)
        {
            var currSpecGroup = SpecializationGroupings.First(sg => sg.Contains(specialization));
            var unplacedCount = totalHeadcount - specGroupHeadcounts.Sum(kvSpecGroup => kvSpecGroup.Value.CurrentCount);

            // Maximumkritérium akkor nem teljesíthető, ha [max létszám <= aktuális], ezen felül az átlagot is ellenőrizzük.
            var specializationGroupMaximum = specGroupHeadcounts[currSpecGroup].MaximumCount <= specGroupHeadcounts[currSpecGroup].CurrentCount;
            var specializationMaximum = specHeadcounts[specialization].MaximumCount <= specHeadcounts[specialization].CurrentCount;
            specializationMaximumAndSameRank = specializationMaximum && specHeadcounts[specialization].MinimumRankAverage <= rankAverage;

            // Specializációcsoportoknál a minimum akkor nem teljesíthető, ha [be nem sorolt hallgatók száma < a minimumra töltéshez szükséges létszám]
            // Figyelembe véve azt az esetet is, ha már minimumlétszám felett van egy specializáció és azt is, ha épp minimumlétszám alattira sorolnánk be a hallgatót.
            specializationGroupMinimum =
                unplacedCount
                < SpecializationGroupings.Sum(sg => Math.Max(0, specGroupHeadcounts[sg].CurrentCount - specGroupHeadcounts[sg].MinimumCount - (sg == currSpecGroup ? 1 : 0)));
            // Ágazatoknál a minimum akkor nem teljesíthető, ha [ágazatokra még besorolható hallgatók száma < ágazatok minimumra töltéséhez szükséges létszám]
            //  - [Ágazatokra még besorolható hallgatók száma == be nem sorolt hallgatók száma - többi specializációcsoport minimumra töltéséhez szükséges létszám]
            //  - Ágazat minimumra töltéséhez szükséges létszámnál figyelembe véve, hogy melyikre sorolnánk éppen a hallgatót
            specializationMinimum =
                unplacedCount - SpecializationGroupings.Where(sg => sg != currSpecGroup).Sum(sg => Math.Max(0, specGroupHeadcounts[sg].MinimumCount - specGroupHeadcounts[sg].CurrentCount))
                < currSpecGroup.Sum(spec => Math.Max(0, specHeadcounts[spec].CurrentCount - specHeadcounts[spec].MinimumCount - (spec == specialization ? 1 : 0)));

            bool result = specializationMinimum && specializationGroupMinimum && specializationMaximum && specializationGroupMaximum && specializationMaximumAndSameRank;
            Log.Write(string.Format("Info: A(z) {0} specializációra (ágazatra) besorolási kísérlet lehetséges: {1}", specialization.Name, result ? "igen" : "nem"));
            if (!result)
                Log.Write(string.Format("Info: Kritériumhiány - specializációcsoport (min: {0}, max: {1}), ágazat (min: {2}, max: {3}, átl: {4})", specializationGroupMinimum, specializationGroupMaximum, specializationMinimum, specializationMaximum, specializationMaximumAndSameRank));
            return result;
        }

        /// <summary>
        /// Becsli egy specializációra (ágazatra) besorolandó minimális hallgatói létszámot a specializációcsoport besorolás közbeni aktuális létszáma alapján.
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
        /// Becsli egy specializációra (ágazatra) besorolható maximális hallgatói létszámot a specializációcsoport besorolás közbeni aktuális létszáma alapján.
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

        /// <summary>
        /// Alaposztály egy specializációcsoport vagy ágazat létszámainak nyilvántartásához.
        /// </summary>
        private class HeadCount
        {
            public int MinimumCount;
            public int MaximumCount;
            public int CurrentCount;
            public bool Closed;

            public HeadCount()
            {
                CurrentCount = 0;
                Closed = false;
            }

            /// <summary>
            /// Lezárja a specializációcsoportot vagy ágazatot; nem engedi, hogy a jelenlegi vagy minimális létszámnál több hallgatót soroljunk be oda.
            /// </summary>
            public void Close()
            {
                MaximumCount = Math.Max(MinimumCount, CurrentCount);
                Closed = true;
            }
        }

        /// <summary>
        /// Egy specializáció (ágazat) létszámait tartja nyilván, figyelembe véve azt, hogy azonos átlagú hallgatók nem szoríthatják ki egymást.
        /// </summary>
        private class SpecializationHeadcount : HeadCount
        {
            public int AboveMinimumCount;
            public double MinimumRankAverage;

            public SpecializationHeadcount(Specialization spec, SpecializationGrouping specGroup, int totalHeadcount)
            {
                MinimumCount = GetSpecMinCount(spec, specGroup, 0, totalHeadcount);
                MaximumCount = GetSpecMaxCount(spec, specGroup, 0, totalHeadcount, totalHeadcount);
                AboveMinimumCount = 0;
                MinimumRankAverage = double.MaxValue;
            }

            /// <summary>
            /// Lezárja a specializációt (ágazatot) az a megadott létszámnál.
            /// </summary>
            /// <param name="value">A lezárás létszáma.</param>
            public void CloseAt(int value)
            {
                MaximumCount = value;
                Closed = true;
            }
        }

        /// <summary>
        /// Egy specializációcsoport létszámait tartja nyilván.
        /// </summary>
        private class SpecializationGroupingHeadcount : HeadCount
        {
            public SpecializationGroupingHeadcount(SpecializationGrouping specGroup, int totalCount)
            {
                MinimumCount = specGroup.GetMinCount(totalCount);
                MaximumCount = specGroup.GetMaxCount(totalCount);
            }
        }
    }
}
