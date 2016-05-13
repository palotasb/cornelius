using Cornelius.Data;

namespace Cornelius.Criteria.Credit
{
    /*
     * Egyszerű szűrőfeltétel tárgykód és félév alapján,
     * amennyiben a megadott tartományon belül történt a tárgy teljesítése,
     * illetve ha a tárgykód megegyezik, akkor a csoportba tartozik a tárgy.
     */
    class MatchCourse : IGroupMatch
    {
        /*
         * Keresett tárgykód
         */
        public string Code;

        /*
         * Kezdeti félév, vagy null, ha nincs alsó határ
         */
        public Semester? From;

        /*
         * Végső félév, vagy null, ha nincs felső határ
         */
        public Semester? To;

        public MatchCourse(string code, Semester? from, Semester? to)
        {
            this.Code = code;
            this.From = from;
            this.To = to;
        }

        /*
         * Feltétel ellenőrzése. 
         */
        public bool Match(Course course)
        {
            return course.Code == this.Code && Semester.InInterval(course.EffectiveSemester, this.From, this.To);
        }
    }
}
