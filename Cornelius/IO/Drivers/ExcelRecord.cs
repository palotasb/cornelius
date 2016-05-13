using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NPOI.SS.UserModel;

namespace Cornelius.IO.Drivers
{
    class ExcelRecord : IRecord
    {
        protected IRow _row;

        public ExcelRecord(IRow row)
        {
            _row = row;
        }

        public int Count
        {
            get 
            {
                return _row.Cells.Count;
            }
        }

        public string this[int i]
        {
            get
            {
                ICell cell = _row.Cells[i];
                if (cell.CellType == CellType.NUMERIC)
                {
                    if (DateUtil.IsCellDateFormatted(cell))
                    {
                        return cell.DateCellValue.ToString();
                    }
                    else
                    {
                        return cell.NumericCellValue.ToString();
                    }
                }
                else
                {
                    return cell.StringCellValue;
                }
            }
        }
    }
}
