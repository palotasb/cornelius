using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cornelius.IO.Tables
{
    [Table(Name = "choices")]
    class XChoice : AbstractRow
    {
        [Column(Name = "user", Number = 2)]
        public XUser User;

        [Column(Name = "number", Number = 3)]
        public int Number;

        [Column(Name = "specialization", Number = 4)]
        public string Specialization;

        public XChoice(XUser user, int number, string choice)
        {
            this.User = user;
            this.Number = number;
            this.Specialization = choice;
        }
    }
}
