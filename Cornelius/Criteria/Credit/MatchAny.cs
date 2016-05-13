using Cornelius.Data;

namespace Cornelius.Criteria.Credit
{
    /*
     * Az alapértelmezett csoportok eleme, minden kurzust elfogad.
     */
    class MatchAny : IGroupMatch
    {
        /*
         * Nem nézzük, mi az, csak elfogadjuk.
         */
        public bool Match(Course course)
        {
            return true;
        }
    }
}
