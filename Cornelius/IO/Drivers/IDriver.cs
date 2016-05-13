using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cornelius.IO.Drivers
{
    interface IDriver
    {
        string Name { get; }
        string[] Extensions { get; }
    }
}
