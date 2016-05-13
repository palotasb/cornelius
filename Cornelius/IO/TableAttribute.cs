using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cornelius.IO
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    sealed class TableAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
