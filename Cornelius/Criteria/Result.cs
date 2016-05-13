using System.Collections.Generic;
using System.Linq;
using Cornelius.Data;

namespace Cornelius.Criteria
{
    /*
     * Egy kritériumellenőrzés eredményét tárolja 
     * mindenféle ember által olvasható adattal együtt.
     */
    class Result
    {
        /*
         * A kritérium végeredménye.
         */
        public bool Value;

        /*
         * A kritérium neve.
         */
        public string Name;

        /*
         * Aleredmények, amiknek a következménye ez.
         */
        public List<Result> Subresults = new List<Result>();

        /*
         * Nem teljesült kritérium esetén ennyi hiányzó kritériumnak számít.
         */
        public int Weight;

        /*
         * Kreditek száma (összegyájti a megfelelően hozzáadott aleredmények kreditjeit is).
         */
        public double Credit = 0;

        /*
         * Pontok (kredit x jegy) száma (összegyájti a megfelelően hozzáadott aleredmények pontjait is).
         */
        public double Points = 0;

        /*
         * Kurzusok, amiket ez, vagy valamelyik aleredmény felhasznált.
         */
        public List<Course> Courses = new List<Course>();

        /*
         * Rendezéshez a kritériumhoz tartozó félév.
         */
        public int? Semester;

        /*
         * A kritériumfából számolható súlyozott átlag.
         */
        public double Avarage
        {
            get
            {
                return this.Credit == 0.0 ? 0.0 : this.Points / this.Credit;
            }
        }

        /*
         * Neve van, de még nem teljesült.
         */
        public Result(string name)
        {
            this.Name = name;
        }

        /*
         * Kurzus adja az alapját a teljesülésnek,
         * ami külön rendelkezés hiányában beleszámít az átlagba is.
         */
        public Result(Course course, bool excludeFromAvarage = false)
        {
            this.Name = course.Name + " (" + course.Code + ")";
            this.Value = true;
            this.Weight = 1;
            if (!excludeFromAvarage)
            {
                this.Credit = course.Credit;
                this.Points = course.Credit * course.Grade;
            }
            this.Courses.Add(course);
        }

        /*
         * Kritérium, ami vagy teljesült vagy nem.
         */
        public Result(string name, bool value)
        {
            this.Name = name;
            this.Value = value;
            this.Weight = value ? 1 : 0;
        }

        /*
         * Alkritériumok hozzáadása, az átlagszámításhoz 
         * szükséges adatok összegyűjtésével együtt.
         */
        public static Result operator+(Result result, Result subresult)
        {
            result.Subresults.Add(subresult);
            result.Credit += subresult.Credit;
            result.Points += subresult.Points;
            result.Courses = result.Courses.Union(subresult.Courses).ToList();
            return result;
        }

        /*
         * Kiértékelhetővé teszi a feltételt különböző helyeken.
         */
        public static implicit operator bool(Result result)
        {
            return result.Value;
        }

        /*
         * Segíti a debuggolást.
         */
        public override string ToString()
        {
            return this.Name + " (weight: " + this.Weight.ToString() + ", value: " + this.Value.ToString() + ")";
        }
    }
}
