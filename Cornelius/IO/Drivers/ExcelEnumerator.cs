using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NPOI.SS.UserModel;

namespace Cornelius.IO.Drivers
{
    class ExcelEnumerator : IEnumerator<IRecord>
    {
        protected IEnumerator _inner;

        public ExcelEnumerator(ISheet sheet)
        {
            _inner = sheet.GetRowEnumerator();
        }

        public IRecord Current
        {
            get 
            { 
                return new ExcelRecord((IRow)_inner.Current); 
            }
        }

        object System.Collections.IEnumerator.Current
        {
            get
            { 
                return this.Current; 
            }
        }

        public bool MoveNext()
        {
            return _inner.MoveNext();
        }

        public void Reset()
        {
            _inner.Reset();
        }

        public void Dispose() { }
    }
}
