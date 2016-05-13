using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cornelius.IO.Drivers
{
    public enum StructuredQueryType
    { 
        Insert,
        TruncateInsert,
        UpdateWithFirstAsKey
    }

    class StructuredQueryDriver : IOutputDriver
    {
        

        public string Name
        {
            get
            {
                return "Structured Query Language";
            }
        }

        public string[] Extensions
        {
            get
            {
                return new string[] { 
                    "sql"
                };
            }
        }

        public StructuredQueryType QueryType { get; set; }
        public Encoding Encoding { get; set; }
        public bool Append { get; set; }
        
        public StructuredQueryDriver()
        {
            this.QueryType = StructuredQueryType.TruncateInsert;
            this.Encoding = Encoding.UTF8;
            this.Append = true;
        }

        public virtual IOutputWriter GetWriter(string path, string name, IEnumerable<string> columns)
        {
            return new StructuredQueryWriter(name, columns, this.QueryType, path, this.Append, this.Encoding);
        }

    }
}
