using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cornelius.Data
{
    /*
     * Egy hallgató által felvett kurzus.
     */
    class Course
    {
        /*
         * Tantárgykód
         */
        public string Code;

        /*
         * Tárgy teljes neve
         */
        public string Name;

        /*
         * Osztályzat
         */
        public int Grade;

        /*
         * Kreditérték
         */
        public double Credit;

        /*
         * Teljesítve van-e a tárgy
         */
        public bool HasCompleted;

        /*
         * Van-e aláírás a tárgyból
         */
        public bool HasSignature;

        /*
         * Az a félév, amikor teljesítette a tárgyat.
         */
        public Semester? EffectiveSemester;
    }
}
