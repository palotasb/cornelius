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
    /*
     * Egy szakirányra jelentkezett hallgató.
     */
    class Student
    {
        /*
         * Egyedi azonosító az adatok egymáshoz kapcsolásához
         */
        public string OriginKey
        {
            get
            {
                return this.Origin + " / " + this.Neptun;
            }
        }

        /*
         * Egyedi azonosító exportáláskor a kulcsok társításához
         */
        public string GroupKey
        {
            get
            {
                return this.Group + " / " + this.Neptun;
            }
        }

        /*
         * Az eredeti, mindenféle átsorolás előtti képzéskód
         */
        public string Origin;

        /*
         * Az aktív képzéskód, ami alapján a kritériumokat számoljuk
         */
        public string Group;

        /*
         * A hallgató Neptun kódja
         */
        public string Neptun;

        /*
         * A hallgató neve
         */
        public string Name;

        /*
         * Felvétel éve
         */
        public Semester EffectiveSemester;

        /*
         * A hallgató kurzusai
         */
        public Course[] Courses;

        /*
         * A hallgató személyes adatai
         */
        public string[] Emails;

        /*
         * A hallgató szakirányjelentkezései
         */
        public string[] Choices;

        /*
         * A hallgató kreditjeinek száma az egyes csoportokban
         */
        public Dictionary<string, double> CreditPerGroup;

        /*
         * A kritériumellenőrzés eredménye
         */
        public Result Result;

        /*
         * Hiányzó kritériumok száma
         */
        public int MissingCriteria;
        
        /*
         * Hanyadik körben lehet besorolni 
         */
        public int Round;

        /*
         * A besorolás eredménye, a hallgató szakiránya
         */
        public Specialization Specialization;
    }
}
