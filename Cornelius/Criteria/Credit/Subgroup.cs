using System.Collections.Generic;
using System.Linq;
using Cornelius.Data;

namespace Cornelius.Criteria.Credit
{
    /// <summary>
    /// Alcsoport, ami összefogja az őt alkotó kurzusok szűrőfeltételeit. Elemként szolgál
    /// a csoportosításokhoz.
    /// </summary>
    class Subgroup : List<IGroupMatch>
    {
        /// <summary>
        /// A csoport azonosítója. Nem tartalmazhat szóközt.
        /// </summary>
        public string Identifier;

        /// <summary>
        /// Túlcsordulnak-e a kreditek?
        /// </summary>
        public bool Overflow = false;

        /// <summary>
        /// Ellenőrzi, hogy egy adott kurzus beletartozik-e a csoportba.
        /// </summary>
        /// <param name="course">A kurzus.</param>
        /// <returns>Igazat ad vissza, ha bármely feltétel teljesül.</returns>
        public bool Match(Course course)
        {
            return this.Any(element => element.Match(course));
        }
    }
}
