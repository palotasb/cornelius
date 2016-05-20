using System.Collections.Generic;
using System.Linq;
using Cornelius.Data;

namespace Cornelius.Criteria.Workflow
{
    /// <summary>
    /// Ötéves kritériumellenőrzés. 
    /// </summary>
    class FiveyearWorkflow : AbstractWorkflow
    {
        /// <summary>
        ///  Közismereti tárgyak ellenőrzése. Ezek beleszámítanak az átlagba.
        /// </summary>
        /// <param name="student">A hallgató.</param>
        protected override void ProcessGroupCriteria(Student student)
        {
            Result result = new Result("Közismereti tárgyak");

            var courses = this.FilterGroupCriteriaCourses(student)
                .Take(this.GroupCriteria.Amount)
                .ToList();

            foreach (var course in courses)
            {
                result += new Result(course);
            }

            result.Value = courses.Count == this.GroupCriteria.Amount;
            result.Weight = result.Value ? this.GroupCriteria.Amount : courses.Count;
            result.Credit = this.GroupCriteria.Amount * 2;

            // Ha nem hiányozhat már annyi tárgya, ami itt hiányzik, akkor nem is sorolható szakirányra
            if (student.Result && !result.Value && (this.GroupCriteria.Amount - courses.Count < student.Result.Weight - this.CourseCriteria.Weight))
            {
                student.Result.Value = false;
            }
            student.Result += result;
            student.Result.Weight += result.Weight;

            Log.Write("Csoportkritérium " + (result.Value ? "elfogadva" : "elutasítva") + ".");
        }

        /// <summary>
        /// A közismereti (kétkredites) tárgyak kiszűrése.
        /// </summary>
        /// <param name="student">A hallgató.</param>
        /// <returns>A közismereti tárgyak listája.</returns>
        public override IEnumerable<Course> FilterGroupCriteriaCourses(Student student)
        {
            return base.FilterGroupCriteriaCourses(student).Where(c => c.Credit == 2);
        }

        /// <summary>
        /// Végső eredmény előállítása.
        /// </summary>
        /// <param name="student">A hallgató.</param>
        protected override void ProcessFinalResult(Student student)
        {
            student.MissingCriteria = this.CourseCriteria.Weight + this.GroupCriteria.Amount - student.Result.Weight;
            if (this.CourseCriteria.Requirement < 0) student.MissingCriteria -= this.CourseCriteria.Requirement;
            if (student.MissingCriteria < 0) student.MissingCriteria = 0;
            student.Round = student.MissingCriteria + 1;
        }
    }
}
