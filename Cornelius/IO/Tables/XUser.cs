using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cornelius.Data;

namespace Cornelius.IO.Tables
{
    [Table(Name = "users")]
    class XUser : AbstractRow
    {
        [Column(Name = "neptun", Number = 2)]
        public string Neptun;

        [Column(Name = "avarage", Number = 3)]
        public double Avarage;

        [Column(Name = "specialization", Number = 4)]
        public string Specialization;

        [Column(Name = "name", Number = 5)]
        public string Name;

        [Column(Name = "group", Number = 6)]
        public string EducationProgram;

        [Column(Name = "accepted", Number = 7)]
        public bool Accepted;

        public bool IsProvisioned;

        public XUser(Student student, int? id = null)
            : base(id)
        {
            this.Neptun = student.Neptun;
            this.IsProvisioned = !id.HasValue;
            this.Avarage = student.Result.Avarage;
            this.Specialization = student.Specialization == null ? null : student.Specialization.Name;
            this.Name = student.Name;
            this.EducationProgram = student.EducationProgram;
            this.Accepted = student.Result;
        }
    }
}
