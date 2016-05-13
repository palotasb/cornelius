using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cornelius.IO
{
    class AbstractRow
    {
        [Column(Name = "id", Number = 1, Sort = true)]
        public int Identifier;

        public AbstractRow(int? id = null)
        {
            if (id.HasValue)
            {
                this.Identifier = id.Value;
            }
            else
            {
                this.Identifier = Provision.UniqueId(this);
            }
        }

        public override string  ToString()
        {
 	         return this.Identifier.ToString();
        }
    }
}
