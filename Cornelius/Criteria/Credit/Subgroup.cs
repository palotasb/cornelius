using System.Collections.Generic;
using System.Linq;
using Cornelius.Data;

namespace Cornelius.Criteria.Credit
{
    /*
     * Alcsoport, ami összefogja az őt alkotó kurzusok szűrőfeltételeit. Elemként szolgál
     * a csoportosításokhoz.
     */
    class Subgroup : List<IGroupMatch>
    {
        /*
         * A csoport azonosítója. Nem tartalmazhat szóközt.
         */
        public string Identifier;

        /*z
         * Túlcsordulnak-e a kreditek?
         */
        public bool Overflow = false;

        /*
         * Ellenőrzi, hogy egy adott kurzus beletartozik-e a csoportba.
         */
        public bool Match(Course course)
        {
            return this.Any(element => element.Match(course));
        }
    }
}
