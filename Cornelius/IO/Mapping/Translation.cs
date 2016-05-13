using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cornelius.IO.Mapping
{
    class Translation : Dictionary<string, string>
    {
        public Transform.Cast ToCast(Type type)
        {
            var cast = Transform.ToType(type);
            return input => (
                input != null && input.Length > 0) ? cast(this[input]) : null;
        }
    }
}
