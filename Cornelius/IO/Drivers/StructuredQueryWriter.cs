using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.IO;

namespace Cornelius.IO.Drivers
{
    class StructuredQueryWriter : IOutputWriter, IDisposable
    {
        public string Path { get; protected set; }

        protected int _line;
        protected StructuredQueryType _queryType;
        protected string _name;
        protected IEnumerable<string> _columns;
        protected StreamWriter _writer;

        public StructuredQueryWriter(string name, IEnumerable<string> columns, StructuredQueryType queryType, string path, bool append, Encoding encoding)
        {
            this.Path = path;
            _line = 0;
            _queryType = queryType;
            _name = name;
            _columns = columns;
            _writer = new StreamWriter(path, append, encoding);
            if (queryType == StructuredQueryType.TruncateInsert) 
            {
                _writer.WriteLine("TRUNCATE TABLE " + _name + ";");
                _writer.WriteLine();
            }
        }

        protected string Escape(string cell)
        {
            if (cell == null)
            {
                return "NULL";
            }

            int intValue;
            if (Int32.TryParse(cell, out intValue) || Int32.TryParse(cell.ToString(), out intValue))
            {
                return intValue.ToString();
            }

            double doubleValue;
            if (Double.TryParse(cell, out doubleValue) || Double.TryParse(cell.ToString(), out doubleValue))
            {
                return String.Format("{0:0.###########}", doubleValue).Replace(",", ".");
            }

            bool boolValue;
            if (Boolean.TryParse(cell, out boolValue))
            {
                return boolValue ? "1" : "0";
            }

            return "'" + Regex.Replace(cell, @"[\r\n\x00\x1a\\'""]", @"\$0") + "'";
        }

        public void WriteRecord(IRecord record)
        {
            if (_queryType == StructuredQueryType.UpdateWithFirstAsKey)
            {
                _writer.Write("UPDATE " + _name + " SET ");
                for (int i = 1; i < record.Count; ++i)
                {
                    _writer.Write("`" + _columns.ElementAt(i) + "` = " + this.Escape(record[i]));
                    if (i < record.Count - 1)
                    {
                        _writer.Write(",");
                    }
                    _writer.Write(" ");
                }
                _writer.WriteLine("WHERE `" + _columns.First() + "` = " + this.Escape(record[0]) + ";");
            }
            else
            {
                if (_line == 20)
                {
                    _writer.WriteLine(";");
                    _writer.WriteLine();
                    _line = 0;
                }
                if (_line == 0)
                {
                    _writer.WriteLine("INSERT INTO `" + _name + "` (`" + String.Join<string>("`, `", _columns) + "`) VALUES");
                }
                else
                {
                    _writer.WriteLine(",");
                }
                _line++;
                _writer.Write("(" + String.Join<string>(", ", record.AsEnumerable().Select(cell => this.Escape(cell))) + ")");
            }
        }

        public void SaveFile()
        {
            if (_queryType != StructuredQueryType.UpdateWithFirstAsKey)
            {
                _writer.WriteLine(";");
            }
            _writer.Flush();
            _writer.Close();
        }

        public void Dispose()
        {
            _writer.Dispose();
        }
    }
}
