using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cornelius.Data
{
    /// <summary>
    /// Egy specializáción belüli ágazat illetve tanszék, ahová be lehet sorolni a hallgatókat.
    /// </summary>
    class Specialization
    {
        /// <summary>
        /// A specializáció neve.
        /// </summary>
        public string Name;

        /// <summary>
        /// A csoport, amibe a specializáció (azaz itt ágazat) tartozik, amin belül további
        /// létszámkövetelményeknek kell megfelelni.
        /// </summary>
        public string SpecializationGroup;

        /// <summary>
        /// Kezdeti helyek aránya az összes jelentkezőhöz viszonyítva.
        /// </summary>
        public double MaxRatio;

        /// <summary>
        /// Minimum helyek aránya az összes jelentkezőhöz viszonyítva.
        /// </summary>
        public double MinRatio;

        /// <summary>
        /// A specializáció maximális létszáma.
        /// </summary>
        public int Capacity;

        /// <summary>
        /// Az utolsó besorolt hallgató köre.
        /// </summary>
        public int Round;

        /// <summary>
        /// Besorolt hallgatók.
        /// </summary>
        public List<Student> Students = new List<Student>();

        /// <summary>
        /// Minimális rangsorátlag, ami a bekerüléshez kell.
        /// </summary>
        public double MinimumRankAverage
        {
            get
            {
                if (this.Students.Count > 0)
                {
                    return this.Students.Select(student => student.Result.Avarage).Min();
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// A specializáció megtelt.
        /// </summary>
        public bool Full
        {
            get
            {
                return this.Students.Count >= this.Capacity;
            }
        }

        /// <summary>
        /// Hallgató besorolása a specializációra.
        /// </summary>
        /// <param name="slot">A specializáció, amire a hallgató besorolódik.</param>
        /// <param name="student">A hallgató, aki besorolásra kerül.</param>
        /// <returns>A </returns>
        public static Specialization operator +(Specialization slot, Student student)
        {
            slot.Students.Add(student);
            student.Specialization = slot;
            slot.Round = student.Round;
            return slot;
        }

        /// <summary>
        /// A specializációra besorolandó minimális hallgatói számot adja vissza.
        /// </summary>
        /// <param name="studentCount">A specilaizációcsoportba besorolható hallgatók minimális száma.</param>
        /// <returns>A specializációra sorolandó minimális létszám.</returns>
        public int GetMinCount(int studentCount)
        {
            return (int)Math.Floor(studentCount * MinRatio);
        }

        /// <summary>
        /// A specializációba besorolható maximális hallgatói számot adja vissza.
        /// </summary>
        /// <param name="studentCount">A specilaizációcsoportba besorolható hallgatók maximális száma.</param>
        /// <returns>A specializációra sorolható maximális létszám.</returns>
        public int GetMaxCount(int studentCount)
        {
            return Math.Min(Capacity, (int)Math.Ceiling(studentCount * MaxRatio));
        }
    }
}
