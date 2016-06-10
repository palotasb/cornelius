using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cornelius.Data;

namespace Cornelius.IO.Reports
{
    [Table(Name = "Szakirányok")]
    class XSumma
    {
        [Column(Name = "Képzéskód", Number = 1, Sort = true)]
        public string EducationProgram;

        [Column(Name = "Szakirány neve", Number = 2)]
        public string Name;

        [Column(Name = "Létszámkorlát", Number = 3)]
        public int Capacity;

        [Column(Name = "Besorolt hallgatók", Number = 4)]
        public int NumberOfStudents;

        [Column(Name = "Legalacsonyabb átlag", Number = 5)]
        public double MinimumAvarage;

        [Column(Name = "Átlag", Number = 6)]
        public double Avarage;

        public XSumma(Specialization specialization, SpecializationGrouping specializationGroup)
        {
            this.EducationProgram = specializationGroup.EducationProgram;
            this.Name = specialization.Name;
            this.Capacity = specialization.Capacity;
            this.NumberOfStudents = specialization.Students.Count;
            this.MinimumAvarage = specialization.MinimumRankAverage;
            this.Avarage = specialization.Students.Average(s => s.Result.Avarage);
        }
    }
}
