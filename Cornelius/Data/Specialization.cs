using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cornelius.Data
{
    /*
     * Egy szakiránybesorolási fiókot valósít meg, ahová
     * be lehet sorolni a hallgatókat.
     */
    class Specialization
    {
        /*
         * A képzés, ahol elindul a szakirány.
         */
        public string Group;

        /*
         * A szakirány neve.
         */
        public string Name;

        /*
         * Kezdeti helyek aránya az összes jelentkezőhöz
         */
        public double Ratio;

        /*
         * Szabad helyek száma.
         */
        public int Capacity;

        /*
         * Az utolsó besorolt hallgató köre.
         */
        public int Round;

        /*
         * Besorolt hallgatók.
         */
        public List<Student> Students = new List<Student>();

        /*
         * Minimális szakirányátlag.
         */
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

        /*
         * Tele van-e a szakirány?
         */
        public bool Full
        {
            get
            {
                return this.Students.Count >= this.Capacity;
            }
        }

        /*
         * Hallgató hozzáadása a szakirányhoz.
         */
        public static Specialization operator +(Specialization slot, Student student)
        {
            slot.Students.Add(student);
            student.Specialization = slot;
            slot.Round = student.Round;
            return slot;
        }
    }
}
