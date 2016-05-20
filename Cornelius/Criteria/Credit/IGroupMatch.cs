using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cornelius.Data;

namespace Cornelius.Criteria.Credit
{
    /// <summary>
    /// A tárgyak csoportokba rendezése ezen az interfészen keresztül működik.
    /// </summary>
    interface IGroupMatch
    {
        /// <summary>
        /// Igaz, ha a csoportba tartozik a tárgy.
        /// </summary>
        /// <param name="course">A vizsgált tárgy.</param>
        /// <returns>A tárgy csoportba tartozósága.</returns>
        bool Match(Course course);
    }
}
