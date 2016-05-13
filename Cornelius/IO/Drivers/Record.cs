using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cornelius.IO.Drivers
{
    class Record : IRecord
    {
        public IEnumerable<string> Enumerable { get; protected set; }

        public Record(IEnumerable<string> contents)
        {
            Enumerable = contents;
        }

        public int Count
        {
            get 
            {
                return Enumerable.Count();
            }
        }

        public string this[int i]
        {
            get
            {
                return Enumerable.ElementAt(i);
            }
        }
    }
}
