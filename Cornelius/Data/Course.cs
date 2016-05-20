using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cornelius.Data
{
    /// <summary>
    /// Egy hallgató által felvett kurzus.
    /// </summary>
    class Course
    {
        /// <summary>
        /// Tantárgykód
        /// </summary>
        public string Code;

        /// <summary>
        /// Tárgy teljes neve
        /// </summary>
        public string Name;

        /// <summary>
        /// Osztályzat
        /// </summary>
        public int Grade;

        /// <summary>
        /// Kreditérték
        /// </summary>
        public double Credit;

        /// <summary>
        /// A tantárgyat teljesítette a hallgató
        /// </summary>
        public bool HasCompleted;

        /// <summary>
        /// A tantárgyból aláírással rendelkezik a hallgató
        /// </summary>
        public bool HasSignature;

        /// <summary>
        /// A teljesítés féléve.
        /// </summary>
        public Semester? EffectiveSemester;
    }
}
