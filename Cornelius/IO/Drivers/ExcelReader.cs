using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace Cornelius.IO.Drivers
{
    class ExcelReader : IInputReader
    {
        protected HSSFWorkbook _workbook;
        protected ISheet _sheet;

        public ExcelReader(string path, string name)
        {
            using (FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                _workbook = new HSSFWorkbook(file);
                if (name == null)
                {
                    _sheet = _workbook.GetSheetAt(0);
                }
                else
                {
                    _sheet = _workbook.GetSheet(name);
                }
            }
        }

        public IEnumerator<IRecord> GetEnumerator()
        {
            return new ExcelEnumerator(_sheet);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Dispose()
        {
            _sheet.Dispose();
            _workbook.Dispose();
        }

        public IRecord FirstRow
        {
            get 
            { 
                return new ExcelRecord(_sheet.GetRow(0));
            }
        }
    }
}
