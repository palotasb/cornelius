using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cornelius.Data
{
    /// <summary>
    /// Egy specializáció, ahová
    /// be lehet sorolni a hallgatókat.
    /// </summary>
    class Specialization
    {
        /// <summary>
        /// A képzés kódja, ahol elindul a szakirány.
        /// </summary>
        public string EducationProgram;

        /// <summary>
        /// A specializáció neve.
        /// </summary>
        public string Name;

        /// <summary>
        /// Kezdeti helyek aránya az összes jelentkezőhöz
        /// </summary>
        public double Ratio;

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
        public double Minimum
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
    }
}
