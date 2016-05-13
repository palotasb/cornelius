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
    /*
     * Kritériumellenőrzés folyamatát megvalósító alaposztály,
     * egy kritériumrendszert reprezentál.
     */
    abstract class AbstractWorkflow
    {
        /*
         * A képzés, amihez a kritériumrendszer tartozik.
         */
        public string Group
            { get; protected set; }

        /*
         * A felvételi félév, amitől kezdve a kritériumrendszer érvényes,
         * vagy null, ha nincs alsó határ.
         */
        public Semester? From
            { get; protected set; }

        /*
         * A felvételi félév, ameddig a kritériumrendszer érvényes,
         * vagy null, ha nincs felső határ.
         */
        public Semester? To
            { get; protected set; }

        /*
         * A tárgyak alapértelmezett csoportosítása.
         */
        public Grouping DefaultGrouping
            { get; protected set; }

        /*
         * A képzéshez tartozó tárgykritériumok.
         */
        public MatchGroup CourseCriteria
            { get; protected set; }

        /*
         * A képzéshez tartozó megkövetelt tárgytípus. 
         * (pl. kötelezően választható tárgyak BSc esetén,
         * vagy a közismereti tárgyak ötéves villamosmérnökin)
         */
        public GroupRequirement GroupCriteria
            { get; set; }

        /*
         * A képzéshez tartozó kreditszám követelmények. 
         * (pl. 120 kredites ellenőrzés csoportokra bontva)
         */
        public List<GroupRequirement> SummaCriteria
            { get; protected set; }

        /*
         * A hallgatókra vonatkozó kritérium beállítása, ami megmondja,
         * mikor tartozik valakihez ez a kritériumrendszer.
         */
        public void SetStudentCriteria(string group, Semester? from, Semester? to)
        {
            this.Group = group;
            this.From = from;
            this.To = to;
        }

        /*
         * Tárgykritériumok betöltése fájlból.
         */
        public void SetCourseCriteria(string criteriaFile)
        {
            this.CourseCriteria = CriteriaDefinitionLanguageParser.Parse(Path.Combine(Program.CONFIG_DIRECTORY, criteriaFile));
        }

        /*
         * Csoportosítás betöltése fájlból.
         */
        public void SetDefaultGrouping(string groupingFile)
        {
            this.DefaultGrouping = GroupingDefinitionLanguageParser.Parse(Path.Combine(Program.CONFIG_DIRECTORY, groupingFile));
        }

        /*
         * Megvizsgálja, hogy a hallgatóra ez a kritériumrendszer vonatkozik-e.
         */
        public bool Match(Student student, bool original = false)
        {
            return (original ? student.Origin : student.Group) == this.Group && Semester.InInterval(student.EffectiveSemester, this.From, this.To);
        }

        /*
         * Hallgatóra vonatkozó kritériumrendszer kiértékelése.
         */
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

        /*
         * Tárgykritériumok kiértékelése.
         */
        protected virtual void ProcessCourseRequirements(Student student)
        {
            student.Result = this.CourseCriteria.Evaluate(new Proxy(student.Courses, student.Origin));
            Log.Write("Tárgyteljesítési kritérium " + (student.Result ? "elfogadva" : "elutasítva") + ".");
        }

        /*
         * Megkövetelt tárgycsoport kiértékelése.
         */
        protected virtual void ProcessGroupCriteria(Student student)
        {
            throw new NotImplementedException();
        }

        /*
         * Megkövetelt kreditszám kiértékelése.
         */
        protected virtual void ProcessSummaCriteria(Student student)
        {
            throw new NotImplementedException();
        }

        /*
         * Azon tárgyak kiválasztása, amik még nem lettek felhasználva kritériumhoz.
         */
        public virtual IEnumerable<Course> FilterGroupCriteriaCourses(Student student)
        {
            return this.FilterCriteriaCourses(student, this.GroupCriteria.Identifier).Except(student.Result.Courses);
        }

        /*
        * Nem 0 kredites tárgyak kiválasztása.
        */
        public virtual IEnumerable<Course> FilterSummaCriteriaCourses(Student student, string group)
        {
            return this.FilterCriteriaCourses(student, group).Where(c => c.Credit > 0);
        }

        /*
         * Teljesített tárgyak kiválasztása eredmény szerint rendezve.
         */
        public virtual IEnumerable<Course> FilterCriteriaCourses(Student student, string group)
        {
            return this.DefaultGrouping.Filter(student.Courses, group)
                .Where(c => c.HasCompleted)
                .OrderByDescending(c => c.Grade);
        }


        /*
         * A végső eredmény feldolgozása, a szakirányra sorolás lehetőségének megteremtése/elvétele
         */
        protected abstract void ProcessFinalResult(Student student);

        protected AbstractWorkflow()
        {
            this.SummaCriteria = new List<GroupRequirement>();
        }
    }
}
