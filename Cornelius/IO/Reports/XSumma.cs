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

        [Column(Name = "Specializáció", Number = 2)]
        public string SpecializationGroup;

        [Column(Name = "Ágazat", Number = 3)]
        public string Name;

        [Column(Name = "Létszámkorlát", Number = 4)]
        public int Capacity;

        [Column(Name = "Besorolt hallgatók", Number = 5)]
        public int NumberOfStudents;

        [Column(Name = "Legalacsonyabb átlag", Number = 6)]
        public double MinimumAvarage;

        [Column(Name = "Átlag", Number = 7)]
        public double Avarage;

        [Column(Name = "Specializáció %", Number = 8)]
        public double SpecializationGroupPercent;

        [Column(Name = "Ágazat %", Number = 9)]
        public double SpecializationPercent;

        public XSumma(Specialization specialization, IEnumerable<SpecializationGrouping> specializationGroups)
        {
            var specGroup = specializationGroups.First(sg => sg.Contains(specialization));
            this.EducationProgram = specGroup.EducationProgram;
            this.SpecializationGroup = specGroup.Key;
            this.Name = specialization.Name;
            this.Capacity = specialization.Capacity;
            this.NumberOfStudents = specialization.Students.Count;
            this.MinimumAvarage = specialization.MinimumRankAverage;
            this.Avarage = specialization.Students.Count != 0 ? specialization.Students.Average(s => s.Result.Avarage) : 0;
            double totalStudents = specializationGroups.Where(sg => sg.EducationProgram == specGroup.EducationProgram).Sum(sg => sg.Sum(spec => spec.Students.Count));
            double specGroupStudents = specGroup.Sum(spec => spec.Students.Count);
            this.SpecializationGroupPercent = 100.0 * ((totalStudents != 0) ? specGroupStudents / totalStudents : 0);
            this.SpecializationPercent = 100.0 * ((specGroupStudents != 0) ? specialization.Students.Count / specGroupStudents : 0);
        }
    }
}
