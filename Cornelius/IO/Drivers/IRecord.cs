using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cornelius.IO.Drivers
{
    interface IRecord
    {
        int Count { get; }
        string this[int i] { get; }
    }
}
