using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cornelius.IO.Drivers
{
    class SeparatedTextWriter : IOutputWriter, IDisposable
    {
        public string Path { get; protected set; }
        public char Separator { get; protected set; }
        public char? Encloser { get; protected set; }
        public Encoding Encoding { get; protected set; }

        protected StreamWriter _writer;

        public SeparatedTextWriter(string path)
            : this(path, ';', '"', Encoding.UTF8) { }

        public SeparatedTextWriter(string path, char separator, char? encloser, Encoding encoding)
        {
            this.Path = path;
            this.Separator = separator;
            this.Encloser = encloser;
            this.Encoding = encoding;
            _writer = new StreamWriter(path, false, encoding);
        }

        public void WriteRecord(IRecord record)
        {
            _writer.WriteLine(String.Join<string>(this.Separator.ToString(), record.AsEnumerable().Select(cell => this.Encloser.HasValue ? this.Encloser.ToString() + (cell == null ? "NULL" : cell) + this.Encloser.ToString() : cell)));
        }

        public void SaveFile()
        {
            _writer.Flush();
            _writer.Close();
        }

        public void Dispose()
        {
            _writer.Dispose();
        }
    }
}
