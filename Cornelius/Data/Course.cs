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
    [System.Diagnostics.DebuggerDisplay("{ToString()}")]
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

        public override string ToString()
        {
            return string.Format("{0} ({1}) {3} {4} Jegy:{5} {2}kr", Name, Code, Credit, HasSignature ? "@" : "-", HasCompleted ? "OK" : "no", Grade);
        }
    }
}
