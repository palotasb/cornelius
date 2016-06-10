using System.Collections.Generic;
using System.Linq;


#pragma warning disable 0649
namespace Cornelius.IO.Primitives
{
    class XSpecialization
    {
        [Map]
        public string Name;

        [Map]
        public string SpecializationGroup;

        [Map]
        public double MaxRatio;

        [Map]
        public double MinRatio;

        [Map]
        public int Capacity;

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
                this.Name == obj.Name &&
                this.SpecializationGroup == obj.SpecializationGroup &&
                this.MaxRatio == obj.MaxRatio &&
                this.MinRatio == obj.MinRatio &&
                this.Capacity == obj.Capacity;
        }

        public override int GetHashCode()
        {
            return
                this.Name.GetHashCode() ^
                this.SpecializationGroup.GetHashCode() ^
                this.MaxRatio.GetHashCode() ^
                this.MinRatio.GetHashCode() ^
                this.Capacity.GetHashCode();
        }
    }
}
