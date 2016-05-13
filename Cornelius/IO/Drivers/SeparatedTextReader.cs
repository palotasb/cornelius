using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cornelius.IO.Drivers
{
    class SeparatedTextReader : IInputReader
    {
        public char Separator { get; protected set; }
        public string Path { get; protected set; }
        public char? Encloser { get; protected set; }

        protected string[] _lines;
        protected readonly char[] _separatorCandidates = new char[] { ';', ',', '\t' };

        public SeparatedTextReader(string path)
        {
            this.Path = path;
            _lines = File.ReadAllLines(this.Path);

            var candidates = new List<char>(_separatorCandidates);
            for (int i = 0; i < _lines.Length && candidates.Count > 1; ++i)
            {
                candidates.RemoveAll(c => !_lines[i].Contains(c));
            }

            if (candidates.Count != 1)
            {
                throw new InvalidDataException("Separator character cannot be recognized.");
            }
            else
            {
                this.Separator = candidates.First();
            }
        }

        public SeparatedTextReader(string path, char separator, char? encloser, Encoding encoding)
        {
            this.Path = path;
            this.Separator = separator;
            this.Encloser = encloser;
            _lines = File.ReadAllLines(this.Path);
        }

        public IEnumerator<IRecord> GetEnumerator()
        {
            return new SeparatedTextEnumerator(_lines, this.Separator, this.Encloser);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Dispose()
        {
            _lines = null;
        }

        public IRecord FirstRow
        {
            get
            {
                return new SeparatedTextRecord(_lines[0], this.Separator, this.Encloser);
            }
        }
    }
}
