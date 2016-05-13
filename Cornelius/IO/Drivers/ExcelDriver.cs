using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cornelius.IO.Drivers
{
    class ExcelDriver : IInputDriver, IOutputDriver
    {
        public string Name
        {
            get
            {
                return "Excel 97-2003";
            }
        }

        public string[] Extensions
        {
            get
            {
                return new string[] { 
                    "xls"
                };
            }
        }

        public IInputReader GetReader(string path, string name)
        {
            return new ExcelReader(path, name);
        }

        public IOutputWriter GetWriter(string path, string name, IEnumerable<string> columns)
        {
            ExcelWriter writer = new ExcelWriter(path, name);
            writer.WriteRecord(columns.AsRecord());
            return writer;
        }
    }
}
