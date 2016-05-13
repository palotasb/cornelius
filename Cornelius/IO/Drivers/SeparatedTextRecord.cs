using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cornelius.IO.Drivers
{
    class SeparatedTextRecord : IRecord
    {
        protected string[] _components;

        public SeparatedTextRecord(string line, char separator, char? encloser)
        {
            _components = line.Split(separator);

            if (!encloser.HasValue)
            {
                encloser = _components[0].First();
            }
            
            if (_components.All(component => encloser == component.First() && encloser == component.Last()))
            {
                _components = _components.Select(component => component.Replace("\\" + encloser.ToString(), encloser.ToString()).Replace("\\\\", "\\").Substring(1, component.Length - 2)).ToArray();
            }
        }

        public int Count
        {
            get
            {
                return _components.Length; 
            }
        }

        public string this[int i]
        {
            get 
            {
                return _components[i];
            }
        }
    }
}
