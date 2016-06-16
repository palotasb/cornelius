using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using Cornelius.Data;

namespace Cornelius.Criteria.Workflow
{
    /// <summary>
    /// 2014 előtti BSc szerinti kritériumellenőrzés.
    /// </summary>
    class BachelorWorkflow : AbstractWorkflow
    {
        /// <summary>
        /// Kötelezően választható tárgyak ellenőrzése. Az átlagba nem szabad, hogy beleszámítsanak.
        /// </summary>
        /// <param name="student">A hallgató.</param>
        protected override void ProcessGroupCriteria(Student student)
        {
            Result result = new Result("Kötelezően választható tárgyak");

            var courses = this.FilterGroupCriteriaCourses(student).Take(this.GroupCriteria.Amount).ToList();
            foreach (var course in courses)
            {
                result += new Result(course);
            }

            result.Value = courses.Count >= this.GroupCriteria.Amount;
            result.Weight = result.Value ? this.GroupCriteria.Amount : courses.Count; 

            // Ha nem hiányozhat már annyi tárgya, ami itt hiányzik, akkor nem is sorolható szakirányra
            if (student.Result && !result.Value && ((this.GroupCriteria.Amount - courses.Count) > (student.Result.Weight - this.CourseCriteria.Weight)))
            {
                student.Result.Value = false;
            }
            student.Result.Weight += result.Weight;
            student.Result.Subresults.Add(result);

            Log.Write("Csoportkritérium " + (result.Value ? "elfogadva" : "elutasítva") + ".");
        }

        /// <summary>
        /// Szumma kreditszám ellenőrzése. Az átlagba nem szabad, hogy beleszámítson.
        /// </summary>
        /// <param name="student">A hallgató</param>
        protected override void ProcessSummaCriteria(Student student)
        {
            Result result = new Result("Kreditkritérium");
            List<Course> globalOverflow = new List<Course>();
            student.CreditPerGroup = new Dictionary<string, double>();
            double remainder = 0;

            foreach (var criteria in this.SummaCriteria)
            {
                // A CamelCase csoportneveket szavakra bontjuk
                string name = Regex.Matches(criteria.Identifier, @"[A-Z][^A-Z]+").OfType<Match>().Select(match => match.Value).Aggregate((acc, b) => acc + " " + b.ToLower()).TrimStart(' ').Replace('_', ' ');
                Result subresult = new Result(name, true);

                // Következő túlcsorduló elemek
                List<Course> localOverflow = new List<Course>();
                bool shouldOverflow = this.DefaultGrouping.First(group => group.Identifier == criteria.Identifier).Overflow;

                double creditCount = 0;
                foreach (Course course in this.FilterSummaCriteriaCourses(student, criteria.Identifier).Union(globalOverflow))
                {
                    if (subresult.Credit < criteria.Amount)
                    {
                        subresult += new Result(course);

                        // Számoljuk, hogy hány kreditet teljesített ilyen tárgyakból
                        creditCount += course.Credit;
                    }
                    else if (shouldOverflow)
                    {
                        // Túlcsordulás
                        localOverflow.Add(course);
                    }
                    else
                    {
                        // Ha nem csordul túl, akkor ezeket is hozzá kell adnunk, hogy teljes legyen a kép
                        creditCount += course.Credit;
                    }
                }

                // Feljegyezzük, hány kreditje volt az adott csoportban
                student.CreditPerGroup.Add(criteria.Identifier, creditCount);

                // Túlcsorduló elemek cseréje
                globalOverflow = localOverflow;

                // Ha több a kredit, levágjuk
                if ((subresult.Credit + remainder) > criteria.Amount)
                {
                    if (shouldOverflow)
                    {
                        // Továbbvisszük a törtkrediteket
                        remainder = subresult.Credit + remainder - criteria.Amount;
                        subresult.Credit = criteria.Amount;
                    }
                    else
                    {
                        subresult.Credit = criteria.Amount;
                        remainder = 0;
                    }
                }
                else if (remainder > 0)
                {
                    subresult.Credit += remainder;
                    remainder = 0;
                }

                result += subresult;
            }

            result.Value = result.Credit >= this.SummaCriteria.Max(c => c.Amount); // A legmagasabb érték egyben az a szám, amit el kell érni. 
            student.Result.Weight += result ? 1 : 0;
            student.Result.Subresults.Add(result); // Ha csak hozzáadnánk a szokásos túlterhelt +-al, akkor beleszámítana az átlagba
            student.Result.Value &= result.Value;

            Log.Write("Kreditkritérium " + (result.Value ? "elfogadva" : "elutasítva") + ".");
        }

        /// <summary>
        /// Ez a kötelezően választható tárgyaknál bővíti ki a szűrést oly módon, hogy az átsorolt hallgatók
        /// az eredeti szakjuknak megfelelően tudjanak beszámítani közismeretiket.
        /// </summary>
        /// <param name="student">A hallgató.</param>
        /// <returns></returns>
        public override IEnumerable<Course> FilterGroupCriteriaCourses(Student student)
        {
            if (student.OriginalEducationProgram == student.EducationProgram)
            {
                return base.FilterGroupCriteriaCourses(student);
            }
            else
            {
                AbstractWorkflow workflow = Evaluator.Match(student, true);
                return workflow.FilterGroupCriteriaCourses(student);
            }
        }

        /// <summary>
        /// A besorolási körök kiszámítása.
        /// </summary>
        /// <param name="student">A hallgató.</param>
        /// <param name="_">Nem használt paraméter.</param>
        protected override void ProcessFinalResult(Student student, IEnumerable<SpecializationGrouping> _)
        {
            student.MissingCriteria = this.CourseCriteria.Weight + this.GroupCriteria.Amount + 1 - student.Result.Weight;
            if (this.CourseCriteria.Requirement < 0) student.MissingCriteria -= this.CourseCriteria.Requirement;
            if (student.MissingCriteria < 0) student.MissingCriteria = 0;
            if (student.OriginalEducationProgram == student.EducationProgram)
            {
                student.Round = 1;
            }
            else
            {
                student.Round = 2 + student.MissingCriteria;
            }
        }
    }
}
