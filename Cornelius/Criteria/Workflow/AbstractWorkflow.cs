using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cornelius.Criteria.Credit;
using Cornelius.Criteria.Expression;
using Cornelius.Data;
using Cornelius.Grammar;

namespace Cornelius.Criteria.Workflow
{
    /// <summary>
    /// Kritériumellenőrzés folyamatát megvalósító alaposztály,
    /// egy kritériumrendszert reprezentál.
    /// </summary>
    abstract class AbstractWorkflow
    {
        /// <summary>
        /// A képzés, amihez a kritériumrendszer tartozik.
        /// </summary>
        public string EducationProgram
            { get; protected set; }

        /// <summary>
        /// A felvételi félév, amitől kezdve a kritériumrendszer érvényes,
        /// vagy null, ha nincs alsó határ.
        /// </summary>
        public Semester? From
            { get; protected set; }

        /// <summary>
        /// A felvételi félév, ameddig a kritériumrendszer érvényes,
        /// vagy null, ha nincs felső határ.
        /// </summary>
        public Semester? To
            { get; protected set; }

        /// <summary>
        /// A tárgyak alapértelmezett csoportosítása.
        /// </summary>
        public Grouping DefaultGrouping
            { get; protected set; }

        /// <summary>
        /// A képzéshez tartozó tárgykritériumok.
        /// </summary>
        public MatchGroup CourseCriteria
            { get; protected set; }

        /// <summary>
        /// A képzéshez tartozó megkövetelt tárgytípus. 
        /// (pl. kötelezően választható tárgyak BSc esetén,
        /// vagy a közismereti tárgyak ötéves villamosmérnökin)
        /// </summary>
        public GroupRequirement GroupCriteria
            { get; set; }

        /// <summary>
        /// A képzéshez tartozó kreditszám követelmények. 
        /// (pl. 120 kredites ellenőrzés csoportokra bontva)
        /// </summary>
        public List<GroupRequirement> SummaCriteria
            { get; protected set; }

        /// <summary>
        /// A hallgatókra vonatkozó kritérium beállítása, ami megmondja,
        /// mikor tartozik valakihez ez a kritériumrendszer.
        /// </summary>
        /// <param name="educationProgram"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void SetStudentCriteria(string educationProgram, Semester? from, Semester? to)
        {
            this.EducationProgram = educationProgram;
            this.From = from;
            this.To = to;
        }

        /// <summary>
        /// Tárgykritériumok betöltése fájlból.
        /// </summary>
        /// <param name="criteriaFile">A kritériumfájl fájlneve (csak bázisnév, elérési út nem kell)</param>
        public void SetCourseCriteria(string criteriaFile)
        {
            this.CourseCriteria = CriteriaDefinitionLanguageParser.Parse(Path.Combine(Program.CONFIG_DIRECTORY, criteriaFile));
        }

        /// <summary>
        /// Csoportosítás betöltése fájlból.
        /// </summary>
        /// <param name="groupingFile">A csoportosításfájl fájlneve (csak bázisnév, elérési út nem kell)</param>
        public void SetDefaultGrouping(string groupingFile)
        {
            this.DefaultGrouping = GroupingDefinitionLanguageParser.Parse(Path.Combine(Program.CONFIG_DIRECTORY, groupingFile));
        }

        /// <summary>
        /// Megvizsgálja, hogy a hallgatóra ez a kritériumrendszer vonatkozik-e.
        /// </summary>
        /// <param name="student">A hallgató</param>
        /// <param name="original">Igaz, ha az eredeti képzéskódját akarjuk a hallgatónak nézni, hamis, ha a jelenlegit.</param>
        /// <returns></returns>
        public bool Match(Student student, bool original = false)
        {
            return (original ? student.OriginalEducationProgram : student.EducationProgram) == this.EducationProgram && Semester.InInterval(student.EffectiveSemester, this.From, this.To);
        }

        /// <summary>
        /// Hallgatóra vonatkozó kritériumrendszer kiértékelése.
        /// </summary>
        /// <param name="student">A hallgató</param>
        /// <param name="exception">Jelzi, ha a hallgató kriérium alól fel van mentve (ekkor az eredmény [Result] igaz lesz).</param>
        public virtual void ProcessStudent(Student student, bool exception)
        {
            Log.Write(student.OriginKey + ":");
            Log.EnterBlock(" => ");
            // A feldolgozás sorrendje is fontos! A tárgykritériumok alól lehet felmentés, így először azokat kell feloldani.
            if (this.CourseCriteria != null)
            {
                this.ProcessCourseRequirements(student);
            }
            if (this.GroupCriteria != null)
            {
                this.ProcessGroupCriteria(student);
            }
            if (this.SummaCriteria.Count > 0)
            {
                this.ProcessSummaCriteria(student);
            }
            this.ProcessFinalResult(student);
            if (exception)
            {
                Log.Write("Kritériumok alól felmentve.");
                student.Result.Value = true;
            }
            if (student.Result) Log.Write(student.Round + ". körben besorolható.");
            else Log.Write("Nem besorolható.");
            Log.LeaveBlock();
        }

        /// <summary>
        /// Tárgykritériumok kiértékelése.
        /// </summary>
        /// <param name="student">A hallgató</param>
        protected virtual void ProcessCourseRequirements(Student student)
        {
            student.Result = this.CourseCriteria.Evaluate(new StudentCourseProxy(student.Courses, student.OriginalEducationProgram));
            Log.Write("Tárgyteljesítési kritérium " + (student.Result ? "elfogadva" : "elutasítva") + ".");
        }

        /// <summary>
        /// Megkövetelt tárgycsoport kiértékelése.
        /// </summary>
        /// <param name="student">A hallgató</param>
        protected virtual void ProcessGroupCriteria(Student student)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Megkövetelt kreditszám kiértékelése.
        /// </summary>
        /// <param name="student">A hallgató.</param>
        protected virtual void ProcessSummaCriteria(Student student)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Azon tárgyak kiválasztása, amik még nem lettek felhasználva kritériumhoz.
        /// </summary>
        /// <param name="student">A hallgató</param>
        /// <returns>A fel nem használt tárgyak listája.</returns>
        public virtual IEnumerable<Course> FilterGroupCriteriaCourses(Student student)
        {
            return this.FilterCriteriaCourses(student, this.GroupCriteria.Identifier).Except(student.Result.Courses);
        }
        
        /// <summary>
        ///  Nem 0 kredites tárgyak kiválasztása.
        /// </summary>
        /// <param name="student">A hallgató</param>
        /// <param name="group">A csoportazonosító.</param>
        /// <returns>A kurzusok listája.</returns>
        public virtual IEnumerable<Course> FilterSummaCriteriaCourses(Student student, string group)
        {
            return this.FilterCriteriaCourses(student, group).Where(c => c.Credit > 0);
        }

        /// <summary>
        /// Teljesített tárgyak kiválasztása eredmény szerint rendezve.
        /// </summary>
        /// <param name="student">A hallgató.</param>
        /// <param name="group">A csoportazonosító.</param>
        /// <returns>A kurzusok listája.</returns>
        public virtual IEnumerable<Course> FilterCriteriaCourses(Student student, string group)
        {
            return this.DefaultGrouping.Filter(student.Courses, group)
                .Where(c => c.HasCompleted)
                .OrderByDescending(c => c.Grade);
        }


        /// <summary>
        /// A végső eredmény feldolgozása, a szakirányra sorolás lehetőségének megteremtése/elvétele.
        /// </summary>
        /// <param name="student"></param>
        protected abstract void ProcessFinalResult(Student student);

        /// <summary>
        /// Kritériumellenőrzés folyamatát megvalósító alaposztály.
        /// </summary>
        protected AbstractWorkflow()
        {
            this.SummaCriteria = new List<GroupRequirement>();
        }
    }
}
