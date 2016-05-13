using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cornelius.IO.Drivers
{
    interface IOutputWriter
    {
        void WriteRecord(IRecord record);
        void SaveFile();
    }
}
