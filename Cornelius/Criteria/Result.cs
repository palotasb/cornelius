using System.Collections.Generic;
using System.Linq;
using Cornelius.Data;

namespace Cornelius.Criteria
{
    /// <summary>
    /// Egy kritériumellenőrzés eredményét tárolja 
    /// mindenféle ember által olvasható adattal együtt.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{ToString()}")]
    class Result
    {
        /// <summary>
        /// A kritérium végeredménye.
        /// </summary>
        public bool Value;

        /// <summary>
        /// A kritérium neve.
        /// </summary>
        public string Name;

        /// <summary>
        /// Aleredmények, amiknek a következménye ez.
        /// </summary>
        public List<Result> Subresults = new List<Result>();

        /// <summary>
        /// Nem teljesült kritérium esetén ennyi hiányzó kritériumnak (kredit vagy tantárgy) számít.
        /// </summary>
        public int Weight;

        /// <summary>
        /// Kreditek száma (összegyájti a megfelelően hozzáadott aleredmények kreditjeit is).
        /// </summary>
        public double Credit = 0;

        /// <summary>
        /// Pontok (kredit x jegy) száma (összegyájti a megfelelően hozzáadott aleredmények pontjait is).
        /// </summary>
        public double Points = 0;

        /// <summary>
        /// Kurzusok, amiket ez, vagy valamelyik aleredmény felhasznált.
        /// </summary>
        public List<Course> Courses = new List<Course>();

        /// <summary>
        /// Rendezéshez a kritériumhoz tartozó félév.
        /// </summary>
        public int? Semester;

        /// <summary>
        /// A kritériumfából számolható súlyozott átlag.
        /// </summary>
        public double Avarage
        {
            get
            {
                return this.Credit == 0.0 ? 0.0 : this.Points / this.Credit;
            }
        }

        /// <summary>
        /// Még nem teljesült, de névvel rendelkező kritérium.
        /// </summary>
        /// <param name="name"></param>
        public Result(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Kurzus adja az alapját a teljesülésnek,
        /// ami külön rendelkezés hiányában beleszámít az átlagba is.
        /// </summary>
        /// <param name="course">A kurzus</param>
        /// <param name="excludeFromAvarage">Igaz, ha az átlagszámításban nem vesszük figyelembe a kurzust.</param>
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

        /// <summary>
        /// Kritérium, ami vagy teljesült vagy nem.
        /// </summary>
        /// <param name="name">A kritérium neve.</param>
        /// <param name="value">A teljesülés (igaz/hamis).</param>
        public Result(string name, bool value)
        {
            this.Name = name;
            this.Value = value;
            this.Weight = value ? 1 : 0;
        }

        /// <summary>
        /// Alkritériumok hozzáadása, az átlagszámításhoz szükséges adatok összegyűjtésével együtt.
        /// </summary>
        /// <param name="result">A kritérium, amihez alkritérium adódik.</param>
        /// <param name="subresult">A hozzáadandó alkritérium.</param>
        /// <returns>A <see cref="Result"/> eredmény a hozzáadott aleredménnyel.</returns>
        public static Result operator+(Result result, Result subresult)
        {
            result.Subresults.Add(subresult);
            result.Credit += subresult.Credit;
            result.Points += subresult.Points;
            result.Courses = result.Courses.Union(subresult.Courses).ToList();
            return result;
        }

        /// <summary>
        /// Kiértékelhetővé teszi a feltételt különböző helyeken.
        /// </summary>
        /// <param name="result">A kritériumeredmény.</param>
        public static implicit operator bool(Result result)
        {
            return result.Value;
        }

        /// <summary>
        /// Kritérium részleges szöveges reprezentációja.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Name + " (weight: " + this.Weight.ToString() + ", value: " + this.Value.ToString() + ")";
        }
    }
}
