using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cornelius.Data
{
    /// <summary>
    /// A specializációk csoportosítása. A szabályzat terminológiájában igazából ez a "specializáció." A <see cref="Key"/>
    /// a csoport (specializáció) neve, a benne foglalt <see cref="Specialization"/> objektumok pedig ágazatok illetve tanszékek.
    /// Az osztály a besorolási algoritmus megvalósításához a létszámarányokat és számokat tartja nyilván.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{ToString()}")]
    class SpecializationGrouping : IGrouping<string, Specialization>
    {
        /// <summary>
        /// Az alárendelt specializációcsoportosítás.
        /// </summary>
        private IGrouping<string, Specialization> Specializations;

        /// <summary>
        /// A specializációcsoport maximális létszámaránya a hallgatók számához képest.
        /// A tényleges maximum a <see cref="GetMaxCount(int)"/> értéke.
        /// </summary>
        public double MaxRatio { get; set; }

        /// <summary>
        /// A specializációcsoport fix maximális létszáma.
        /// A tényleges maximum a <see cref="GetMaxCount(int)"/> értéke.
        /// </summary>
        public int Capacity { get; set; }

        /// <summary>
        /// A specializációcsoport minimális létszámaránya a hallgatók számához képest.
        /// </summary>
        public double MinRatio { get; set; }

        /// <summary>
        /// A szak, emihez a specializációk tartoznak.
        /// </summary>
        public string EducationProgram { get; set; }

        /// <summary>
        /// A specializációcsoportra való bekerüléshez szükséges tárgycsoport neve, vagy ha nincs ilyen, akkor üres karakterlánc.
        /// </summary>
        public string PreSpecializationCourseGroup { get; set; }

        /// <summary>
        /// A specializációcsoportban besorolandó minimális hallgatói számot adja vissza.
        /// </summary>
        /// <param name="studentCount">A besorolható hallgatók száma.</param>
        /// <returns>A csoportba sorolandó minimális létszám.</returns>
        public int GetMinCount(int studentCount)
        {
            return (int)Math.Floor(studentCount * MinRatio);
        }

        /// <summary>
        /// A specializációcsoportban besorolható maximális hallgatói számot adja vissza.
        /// </summary>
        /// <param name="studentCount">A besorolható hallgatók száma.</param>
        /// <returns>A csoportba sorolható maximális létszám.</returns>
        public int GetMaxCount(int studentCount)
        {
            return Math.Min(Capacity, (int)Math.Ceiling(studentCount * MaxRatio));
        }

        /// <summary>
        /// Új objektum inicializálása az <see cref="IGrouping{string, Specialization}"/> alapján, amit
        /// a LINQ <see cref="System.Linq.Enumerable.GroupBy{Specialization, string}(IEnumerable{Specialization}, Func{Specialization, string})"/> visszaad.
        /// </summary>
        /// <param name="specializations"></param>
        public SpecializationGrouping(IGrouping<string, Specialization> specializations)
        {
            Specializations = specializations;
        }

        /// <summary>
        /// A specializációcsoport neve.
        /// </summary>
        public string Key { get { return Specializations.Key; } }

        /// <summary>
        /// A specializációcsoportban lévő specializációkat (ágazatokat) enumerátorát visszaadó függvény.
        /// </summary>
        /// <returns>A <see cref="IEnumerator{Specialization}"/> objektum.</returns>
        public IEnumerator<Specialization> GetEnumerator() { return Specializations.GetEnumerator(); }

        /// <summary>
        /// A specializációcsoportban lévő specializációkat (ágazatokat) enumerátorát visszaadó függvény.
        /// </summary>
        /// <returns>A <see cref="IEnumerator{Specialization}"/> objektum.</returns>
        IEnumerator IEnumerable.GetEnumerator() { return Specializations.GetEnumerator(); }

        public override string ToString()
        {
            return string.Format("{0} ({1:##%}-{2:##%}/{3})", Key, MinRatio, MaxRatio, Capacity);
        }
    }
}
