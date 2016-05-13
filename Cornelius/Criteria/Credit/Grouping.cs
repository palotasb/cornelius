using System.Collections.Generic;
using System.Linq;
using Cornelius.Data;

namespace Cornelius.Criteria.Credit
{
    /**
     * Csoportosítás kreditkritérium ellenőrzéshez
     * Alcsoportok halmaza, amik képesek önállóan vizsgálni, hogy egy adott kurzus beléjük tartozik-e. A vizsgálat
     * sorrendben történik, és ott áll meg, ahol az első találat van. Egy csoportosítás tehát egy tárgyat csak egy csoportba
     * tud besorolni.
     */
    class Grouping : List<Subgroup>
    {
        /*
         * Összetársítja a kurzust a csoporttal, amibe való.
         */
        public string Match(Course course)
        {
            return this.First(group => group.Match(course)).Identifier;
        }

        /*
         * Kiszűri a halmazból azokat a tárgyakat, amik a megadott csoportba tartoznak.
         */
        public IEnumerable<Course> Filter(IEnumerable<Course> courses, string group)
        {
            return courses.Where(course => this.Match(course) == group);
        }
    }
}
