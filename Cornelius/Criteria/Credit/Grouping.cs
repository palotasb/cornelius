using System.Collections.Generic;
using System.Linq;
using Cornelius.Data;

namespace Cornelius.Criteria.Credit
{
    /// <summary>
    /// Csoportosítás kreditkritérium ellenőrzéshez.
    /// Alcsoportok halmaza, amik képesek önállóan vizsgálni, hogy egy adott kurzus beléjük tartozik-e. A vizsgálat
    /// sorrendben történik, és ott áll meg, ahol az első találat van. Egy csoportosítás tehát egy tárgyat csak egy csoportba
    /// tud besorolni.
    /// </summary>
    class Grouping : List<Subgroup>
    {
        /// <summary>
        /// Összetársítja a kurzust a csoporttal, amibe való.
        /// </summary>
        /// <param name="course">A keresett kurzus.</param>
        /// <returns>A csoport azonosítója.</returns>
        protected string Match(Course course)
        {
            return this.First(group => group.Match(course)).Identifier;
        }

        /// <summary>
        /// Kiszűri a halmazból azokat a tárgyakat, amik a megadott csoportba tartoznak.
        /// </summary>
        /// <param name="courses">A kurzusok listája.</param>
        /// <param name="group">A csoportazonosító, amibe a kurzusok tartoznak.</param>
        /// <returns>Azon kurzusok, amik az adott csoporthoz tartoznak.</returns>
        public IEnumerable<Course> Filter(IEnumerable<Course> courses, string group)
        {
            return courses.Where(course => this.Match(course) == group);
        }
    }
}
