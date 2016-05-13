using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cornelius.IO
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    sealed class ColumnAttribute : Attribute
    {
        public string Name { get; set; }
        public int Number { get; set; }
        public bool Sort { get; set; }
    }
}
