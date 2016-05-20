using Cornelius.Data;

namespace Cornelius.Criteria.Credit
{
    /// <summary>
    /// Az alapértelmezett csoportok eleme, minden kurzust elfogad.
    /// </summary>
    class MatchAny : IGroupMatch
    {
        /// <summary>
        /// Nem nézzük, mi az, csak elfogadjuk. (Mindig igazat ad vissza.)
        /// </summary>
        /// <param name="course">Mindegy</param>
        /// <returns>Igazat ad vissza minden esetben.</returns>
        public bool Match(Course course)
        {
            return true;
        }
    }
}
