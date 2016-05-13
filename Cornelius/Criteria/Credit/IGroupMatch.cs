using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cornelius.Data;

namespace Cornelius.Criteria.Credit
{
    /*
     * A tárgyak csoportokba rendezése ezen az interfészen keresztül működik.
     */
    interface IGroupMatch
    {
        /*
         * Igaz, ha a csoportba tartozik a tárgy. 
         */
        bool Match(Course course);
    }
}
