using Cornelius.Data;

namespace Cornelius.Criteria.Credit
{
    /// <summary>
    /// Egyszerű szűrőfeltétel tárgykód és félév alapján,
    /// amennyiben a megadott tartományon belül történt a tárgy teljesítése,
    /// illetve ha a tárgykód megegyezik, akkor a csoportba tartozik a tárgy.
    /// </summary>
    class MatchCourse : IGroupMatch
    {
        /// <summary>
        /// Keresett tárgykód
        /// </summary>
        public string Code;

        /// <summary>
        /// Kezdeti félév, vagy null, ha nincs alsó határ
        /// </summary>
        public Semester? From;

        /// <summary>
        /// Végső félév, vagy null, ha nincs felső határ
        /// </summary>
        public Semester? To;

        /// <summary>
        /// Egyszerű szűrőfeltétel tárgykód és félév alapján.
        /// </summary>
        /// <param name="code">Tárgykód</param>
        /// <param name="from">Félév (-tól)</param>
        /// <param name="to">Félév (-ig)</param>
        public MatchCourse(string code, Semester? from, Semester? to)
        {
            this.Code = code;
            this.From = from;
            this.To = to;
        }

        /// <summary>
        /// Feltétel ellenőrzése. 
        /// </summary>
        /// <param name="course">A kurzus</param>
        /// <returns>Igazat ad vissza, ha teljesül.</returns>
        public bool Match(Course course)
        {
            return course.Code == this.Code && Semester.InInterval(course.EffectiveSemester, this.From, this.To);
        }
    }
}
