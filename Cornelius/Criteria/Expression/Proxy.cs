using System.Collections.Generic;
using Cornelius.Data;
using System.Linq;

namespace Cornelius.Criteria.Expression
{
    /*
     * A hallgató tárgyainak használatát nyilvántartó osztály. Tárolja,
     * hogy melyik tárgyakat számítottuk bele az átlagba és hogy melyeket nem
     * lehet többször felhasználni.
     */
    class Proxy
    {
        /*
         * A képzés, ahol a hallgató elkezdte a tanulmányait
         */
        public string Origin
            { get; protected set; }

        /*
         * A hallgató tárgyai.
         */
        protected IEnumerable<Course> _courses;

        /*
         * A többször fel nem használható tárgyak listája.
         */
        protected List<string> _locked = new List<string>();

        /*
         * A már felhasznált tárgyak listája.
         */
        protected List<string> _used = new List<string>();

        /*
         * Tárgy felhasználása - igazat ad vissza, ha ez volt
         * az első alkalom.
         */
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

        /*
         * Tárgy lezárása - igazat ad vissza, ha nem ütközik
         * másik lezárással.
         */
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

        /*
         * Kurzus lekérdezése.
         */
        public Course Check(string code)
        {
            return _courses.FirstOrDefault(c => c.Code == code);
        }

        public Proxy(IEnumerable<Course> courses, string origin)
        {
            this.Origin = origin;
            _courses = courses;
        }
    }
}
