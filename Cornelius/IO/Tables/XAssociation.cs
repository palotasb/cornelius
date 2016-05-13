using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cornelius.IO.Tables
{
    [Table(Name = "associations")]
    class XAssociation : AbstractRow
    {
        public string Key
        {
            get
            {
                return this.Group + " / " + this.Neptun;
            }
        }

        [Column(Name = "group", Number = 2)]
        public string Group;

        [Column(Name = "neptun", Number = 3)]
        public string Neptun;

        public XAssociation(string neptun, string group, int? id)
            : base(id)
        {
            this.Neptun = neptun;
            this.Group = group;
        }

        public XAssociation()
            : base(0) { }
    }
}
