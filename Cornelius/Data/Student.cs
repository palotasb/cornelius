using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cornelius.Criteria.Expression;
using System.IO;
using Cornelius.Criteria;

namespace Cornelius.Data
{
    /// <summary>
    /// Egy hallgatót reprezentáló osztály.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{ToString()}")]
    class Student
    {
        /// <summary>
        /// Egyedi azonosító az adatok egymáshoz kapcsolásához
        /// </summary>
        public string OriginKey
        {
            get
            {
                return this.OriginalEducationProgram + " / " + this.Neptun;
            }
        }

        /// <summary>
        /// Egyedi azonosító exportáláskor a kulcsok társításához
        /// </summary>
        public string Key
        {
            get
            {
                return this.EducationProgram + " / " + this.Neptun;
            }
        }
        
        /// <summary>
        /// Az eredeti, mindenféle átsorolás előtti képzéskód.
        /// </summary>
        public string OriginalEducationProgram;

        /// <summary>
        /// Az aktív képzéskód, ami alapján a kritériumokat számoljuk.
        /// </summary>
        public string EducationProgram;

        /// <summary>
        /// A hallgató Neptun-kódja.
        /// </summary>
        public string Neptun;

        /// <summary>
        /// A hallgató neve.
        /// </summary>
        public string Name;

        /// <summary>
        /// Felvétel éve.
        /// </summary>
        public Semester EffectiveSemester;

        /// <summary>
        /// A hallgató kurzusai.
        /// </summary>
        public Course[] Courses;

        /// <summary>
        /// A hallgató elérhetőségei.
        /// </summary>
        public string[] Emails;

        /// <summary>
        /// A hallgató jelentkezési sorrendje.
        /// </summary>
        public string[] Choices;

        /// <summary>
        /// A hallgató kreditjeinek száma az egyes csoportokban.
        /// </summary>
        public Dictionary<string, double> CreditPerGroup;

        /// <summary>
        /// A kritériumellenőrzés eredménye.
        /// </summary>
        public Result Result;

        /// <summary>
        /// Hiányzó kritériumok száma.
        /// </summary>
        public int MissingCriteria;
        
        /// <summary>
        /// Hanyadik körben lehet besorolni a hallgatót.
        /// </summary>
        public int Round;

        /// <summary>
        /// A besorolás eredménye, a hallgató specializációja (vagy null, ha - még - nem került besorolásra).
        /// </summary>
        public Specialization Specialization;

        public override string ToString()
        {
            return OriginKey;
        }
    }
}
