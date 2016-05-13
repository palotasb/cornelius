using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cornelius.IO.Mapping
{
    class Assignment
    {
        public int Index;
        public FieldInfo Field;
        public Transform.Cast Cast;

        public Assignment(int index, FieldInfo field)
        {
            this.Index = index;
            this.Field = field;
        }
    }
}
