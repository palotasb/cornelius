using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace Cornelius.IO.Drivers
{
    class ExcelWriter : IOutputWriter, IDisposable
    {
        public string Path { get; protected set; }

        protected HSSFWorkbook _workbook;
        protected ISheet _sheet;
        protected int _currentRow;

        public ExcelWriter(string path, string name = "Sheet")
        {
            this.Path = path;
            if (File.Exists(path))
            {
                using (FileStream stream = File.OpenRead(path))
                {
                    _workbook = new HSSFWorkbook(stream);
                }
                for (int i = 0; i < _workbook.NumberOfSheets; ++i)
                {
                    while (i < _workbook.NumberOfSheets && _workbook.GetSheetAt(i).SheetName == name)
                    {
                        _workbook.RemoveSheetAt(i);
                    }
                }
                _sheet = _workbook.CreateSheet(name);
            }
            else
            {
                _workbook = new HSSFWorkbook();
                _sheet = _workbook.CreateSheet(name);
            }
            _currentRow = 0;
        }

        protected void WriteCell(ICell cell, string content)
        {
            double doubleValue;
            if (content == null)
            {
                cell.SetCellType(CellType.BLANK);
            }
            else if (Double.TryParse(content, out doubleValue) || Double.TryParse(content.ToString(), out doubleValue))
            {
                cell.SetCellType(CellType.NUMERIC);
                cell.SetCellValue(doubleValue);
            }
            else
            {
                cell.SetCellType(CellType.STRING);
                cell.SetCellValue(content);
            }
        }

        public void WriteRecord(IRecord record)
        {
            IRow row = _sheet.CreateRow(_currentRow);
            for (int i = 0; i < record.Count; ++i)
            {
                this.WriteCell(row.CreateCell(i), record[i]);
            }
            _currentRow++;
        }

        public void SaveFile()
        {
            using (FileStream stream = File.OpenWrite(this.Path))
            {
                _workbook.Write(stream);
            }
            _sheet.Dispose();
            _workbook.Dispose();
        }

        public void Dispose()
        {
            _sheet.Dispose();
            _workbook.Dispose();
        }
    }
}
