using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cornelius.IO.Drivers
{
    interface IInputReader : IEnumerable<IRecord>, IDisposable
    {
        IRecord FirstRow { get; }
    }
}
