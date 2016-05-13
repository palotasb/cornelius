using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cornelius.IO.Reports
{
    class XHistogram
    {
        [Column(Name = "Kredit", Number = 1)]
        public double Credit;

        [Column(Name = "Darab", Number = 2)]
        public int Count;

        public XHistogram(double credit, int count)
        {
            this.Credit = credit;
            this.Count = count;
        }
    }
}
