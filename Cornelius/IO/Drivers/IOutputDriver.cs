using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cornelius.IO.Drivers
{
    interface IOutputDriver : IDriver
    {
        IOutputWriter GetWriter(string path, string name, IEnumerable<string> columns);
    }
}
