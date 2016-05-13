using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cornelius.IO.Drivers
{
    interface IInputDriver : IDriver
    {
        IInputReader GetReader(string path, string name);
    }
}
