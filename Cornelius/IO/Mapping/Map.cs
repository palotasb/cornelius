using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cornelius.IO.Mapping
{
    class Map
    {
        public List<string> Files = new List<string>();
        public Dictionary<string, string> Columns = new Dictionary<string,string>();
        public Dictionary<string, Translation> Enums = new Dictionary<string,Translation>();
    }
}
