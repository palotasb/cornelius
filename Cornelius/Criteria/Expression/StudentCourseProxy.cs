using System.Collections.Generic;
using Cornelius.Data;
using System.Linq;

namespace Cornelius.Criteria.Expression
{
    /// <summary>
    /// A hallgató tárgyainak használatát nyilvántartó osztály. Tárolja,
    /// hogy melyik tárgyakat számítottuk bele az átlagba és hogy melyeket nem
    /// lehet többször felhasználni.
    /// </summary>
    class StudentCourseProxy
    {
        public string Origin
        /// <summary>
        /// A képzés, ahol a hallgató elkezdte a tanulmányait
        /// </summary>
            { get; protected set; }

        /// <summary>
        /// A hallgató tárgyai.
        /// </summary>
        protected IEnumerable<Course> _courses;

        /// <summary>
        /// A többször fel nem használható tárgyak listája.
        /// </summary>
        protected List<string> _locked = new List<string>();

        /// <summary>
        /// A már felhasznált tárgyak listája.
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

        public StudentCourseProxy(IEnumerable<Course> courses, string origin)
        /// <summary>
        /// A hallgató tárgyainak használatát nyilvántartó osztály.
        /// </summary>
        /// <param name="courses">A hallgató kurzusai.</param>
        /// <param name="origEduProgramCode">A hallgató eredeti képzéskódja.</param>
        {
            this.Origin = origin;
            _courses = courses;
        }
    }
}
