using System.Collections.Generic;
using System.Linq;


#pragma warning disable 0649
namespace Cornelius.IO.Primitives
{
    class XSpecialization
    {
        [Map]
        public string Group;

        [Map]
        public string Name;

        [Map]
        public double Ratio;

        public override bool Equals(object obj)
        {
            if (obj is XSpecialization)
            {
                return this.Equals((XSpecialization)obj);
            }
            else
            {
                return false;
            }
        }

        public bool Equals(XSpecialization obj)
        {
            return
                this.Group == obj.Group &&
                this.Name == obj.Name &&
                this.Ratio == obj.Ratio;
        }

        public override int GetHashCode()
        {
            return this.Group.GetHashCode() ^
                this.Name.GetHashCode() ^
                this.Ratio.GetHashCode();
        }
    }
}
