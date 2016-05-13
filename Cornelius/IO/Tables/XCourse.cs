using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cornelius.Data;

namespace Cornelius.IO.Tables
{
    [Table(Name = "courses")]
    class XCourse : AbstractRow
    {
        [Column(Name = "result", Number = 2)]
        public XResult Result;

        [Column(Name = "code", Number = 3)]
        public string Code;

        [Column(Name = "name", Number = 4)]
        public string Name;

        [Column(Name = "credit", Number = 5)]
        public double Credit;

        [Column(Name = "grade", Number = 6)]
        public int Grade;

        public XCourse(XResult result, Course course)
        {
            this.Result = result;
            this.Code = course.Code;
            this.Name = course.Name;
            this.Credit = course.Credit;
            this.Grade = course.Grade;
        }
    }
}
