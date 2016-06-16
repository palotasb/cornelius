using System.Collections.Generic;
using Cornelius.Data;
using System.Linq;

namespace Cornelius.Criteria.Expression
{
    /// <summary>
    /// A hallgató tárgyainak használatát nyilvántartó osztály. Tárolja,
    /// hogy melyik tárgyakat használtuk fel a rangsorszámításhoz és hogy
    /// melyeket zártuk le anélkül, hogy felhasználtuk volna.
    /// </summary>
    class StudentCourseProxy
    {
        /// <summary>
        /// A képzés, ahol a hallgató elkezdte a tanulmányait
        /// </summary>
        public string OriginalEducationProgram
            { get; protected set; }

        /// <summary>
        /// A hallgató tárgyai.
        /// </summary>
        protected IEnumerable<Course> _courses;

        /// <summary>
        /// A többször fel nem használható lezárt tárgyak listája.
        /// </summary>
        protected List<string> _locked = new List<string>();

        /// <summary>
        /// A már felhasznált és rangsorszámításhoz figyelembe vett tárgyak listája.
        /// </summary>
        protected List<string> _used = new List<string>();

        /// <summary>
        /// Tárgy felhasználása - igazat ad vissza, ha ez volt
        /// az első alkalom.
        /// </summary>
        /// <param name="code">Tárgykód</param>
        /// <returns>Igazat ad vissza, ha most kerül felhasználásra a tárgy, hamisat, ha már fel van használva.</returns>
        public bool Use(string code)
        {
            if (_used.Contains(code))
            {
                return false;
            }
            else
            {
                _used.Add(code);
                return true;
            }
        }

        /// <summary>
        /// Tárgy lezárása - igazat ad vissza, ha nem ütközik
        /// másik lezárással.
        /// </summary>
        /// <param name="code">Tárgykód</param>
        /// <returns>Igazat ad vissza, ha most kerül lezárásra a tárgy. Hamisat, ha már le van zárva.</returns>
        public bool Lock(string code)
        {
            if (_locked.Contains(code))
            {
                return false;
            }
            else
            {
                _locked.Add(code);
                return true;
            }
        }

        /// <summary>
        /// Kurzus lekérdezése a hallgatóhoz rendelt kurzusok listájából.
        /// </summary>
        /// <param name="code">Tárgykód</param>
        /// <returns>A kurzust adja vissza, ha a hallgató tárgyai között van a keresett tárgykód, és null-t, ha nem.</returns>
        public Course Check(string code)
        {
            return _courses.FirstOrDefault(c => c.Code == code);
        }

        /// <summary>
        /// A hallgató tárgyainak használatát nyilvántartó osztály.
        /// </summary>
        /// <param name="courses">A hallgató kurzusai.</param>
        /// <param name="originalEducationProgram">A hallgató eredeti képzéskódja.</param>
        public StudentCourseProxy(IEnumerable<Course> courses, string originalEducationProgram)
        {
            this.OriginalEducationProgram = originalEducationProgram;
            _courses = courses;
        }
    }
}
