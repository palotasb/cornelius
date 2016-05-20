using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cornelius.Data;
using Cornelius.Criteria;

namespace Cornelius.IO.Reports
{
    [Table(Name = "Hallgatók")]
    class XStudent
    {
        [Column(Name = "Eredeti képzés", Number = 1)]
        public string OriginalEducationProgram;

        [Column(Name = "Besorolva mint", Number = 2)]
        public string EducationProgram;

        [Column(Name = "Neptun kód", Number = 3, Sort = true)]
        public string Neptun;

        [Column(Name = "Név", Number = 4)]
        public string Name;

        [Column(Name = "Hiányzó kritérium", Number = 5)]
        public int MissingCriteria;

        [Column(Name = "Szakirányátlag", Number = 6)]
        public double Avarage;

        [Column(Name = "Szakirány", Number = 7)]
        public string Specialization;

        [Column(Name = "Megjelölt hely", Number = 8)]
        public int Number;

        [Column(Name = "Tanköri foglalkozás", Number = 8)]
        public int Exceptable;

        public XStudent(Student student)
        {
            this.OriginalEducationProgram = student.OriginalEducationProgram;
            this.EducationProgram = student.EducationProgram;
            this.Neptun = student.Neptun;
            this.Name = student.Name;
            this.MissingCriteria = student.MissingCriteria;
            this.Avarage = student.Result.Avarage;
            this.Specialization = student.Specialization == null ? null : student.Specialization.Name;
            if (this.Specialization != null)
            { 
                this.Number = Array.IndexOf<string>(student.Choices, this.Specialization) + 1;
            }
            Result exceptor = student.Result.Subresults.First(r => r.Name == "Tanköri foglalkozás");
            if (exceptor != null)
            {
                this.Exceptable = exceptor.Courses.Count;
            }
            else
            {
                this.Exceptable = 0;
            }
        }
    }
}
