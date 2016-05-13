using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cornelius.IO.Drivers
{
    class SeparatedTextEnumerator : IEnumerator<IRecord>
    {
        public char Separator { get; protected set; }
        public char? Encloser { get; protected set; }
        protected IEnumerator<string> _innerEnumerator; 

        public SeparatedTextEnumerator(IEnumerable<string> lines, char separator, char? encloser)
        {
            this.Separator = separator;
            this.Encloser = encloser;
            _innerEnumerator = lines.GetEnumerator();
        }

        public IRecord Current
        {
            get { return new SeparatedTextRecord(_innerEnumerator.Current, this.Separator, this.Encloser); }
        }

        public void Dispose()
        {
            _innerEnumerator.Dispose();
        }

        object System.Collections.IEnumerator.Current
        {
            get { return this.Current; }
        }

        public bool MoveNext()
        {
            return _innerEnumerator.MoveNext();
        }

        public void Reset()
        {
            _innerEnumerator.Reset();
        }
    }
}
