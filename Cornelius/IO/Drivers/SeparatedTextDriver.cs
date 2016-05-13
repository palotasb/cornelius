using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cornelius.IO.Drivers
{
    class SeparatedTextDriver : IInputDriver, IOutputDriver
    {
        public string Name
        {
            get
            {
                return "Separated Text";
            }
        }

        public string[] Extensions
        {
            get 
            {
                return new string[] {
                    "tab",
                    "txt",
                    "csv"
                };
            }
        }

        public IInputReader GetReader(string path, string name)
        {
            return new SeparatedTextReader(path);
        }

        public IOutputWriter GetWriter(string path, string name, IEnumerable<string> columns)
        {
            SeparatedTextWriter writer = new SeparatedTextWriter(path);
            writer.WriteRecord(columns.AsRecord());
            return writer;
        }
    }
}
