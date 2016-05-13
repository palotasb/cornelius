using System;
using System.Reflection;

namespace Cornelius.IO
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    sealed class MapAttribute : Attribute
    {
        public bool Required { get; set; }

        public MapAttribute()
        {
            this.Required = true;
        }
    }
}
