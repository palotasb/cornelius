using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cornelius.Data;

namespace Cornelius.IO.Tables
{
    [Table(Name = "avarages")]
    class XAvarage : AbstractRow
    {
        [Column(Name = "user", Number = 2)]
        public XUser User;

        [Column(Name = "code", Number = 3)]
        public string Code;

        [Column(Name = "name", Number = 4)]
        public string Name;

        [Column(Name = "credit", Number = 5)]
        public double Credit;

        [Column(Name = "grade", Number = 6)]
        public int Grade;

        public XAvarage(XUser user, Course course)
        {
            this.User = user;
            this.Name = course.Name;
            this.Code = course.Code;
            this.Credit = course.Credit;
            this.Grade = course.Grade;
        }

        public XAvarage(XUser user, double missingCredits)
        {
            this.User = user;
            this.Name = "Hiányzó kreditek";
            this.Code = "SZAKHIÁNYZÓ";
            this.Credit = missingCredits;
            this.Grade = 0;
        }
    }
}
