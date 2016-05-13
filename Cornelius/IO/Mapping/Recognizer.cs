using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Cornelius.IO.Drivers;

namespace Cornelius.IO.Mapping
{
    class Recognizer<T> : List<T>
        where T : new()
    {
        public Map Map { get; protected set; }
        
        public Recognizer(Map map)
        {
            this.Map = map;
        }

        public bool Recognize(string path)
        {
            if (Driver.CanReadFile(path))
            {
                foreach (string pattern in this.Map.Files)
                {
                    if (path.Contains(pattern))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public IEnumerable<T> Parse(string path)
        {
            List<T> list = MappedReader.Read<T>(path, this.Map).ToList();
            this.AddRange(list.Except(this));
            return this.Where(elem => list.Contains(elem));
        }
    }
}
