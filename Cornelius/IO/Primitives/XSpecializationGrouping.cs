using System.Collections.Generic;
using System.Linq;


#pragma warning disable 0649
namespace Cornelius.IO.Primitives
{
    class XSpecializationGrouping
    {
        [Map]
        public string EducationProgram;

        [Map]
        public string Name;

        [Map]
        public string PreSpecializationCourseGroup;

        [Map]
        public double MinRatio;
        
        [Map]
        public double MaxRatio;

        [Map]
        public int Capacity;

        public override bool Equals(object obj)
        {
            if (obj is XSpecializationGrouping)
            {
                return this.Equals((XSpecializationGrouping)obj);
            }
            else
            {
                return false;
            }
        }

        public bool Equals(XSpecializationGrouping obj)
        {
            return
                this.EducationProgram == obj.EducationProgram &&
                this.Name == obj.Name &&
                this.PreSpecializationCourseGroup == obj.PreSpecializationCourseGroup &&
                this.MaxRatio == obj.MaxRatio &&
                this.MinRatio == obj.MinRatio &&
                this.Capacity == obj.Capacity;
        }

        public override int GetHashCode()
        {
            return
                this.EducationProgram.GetHashCode() ^
                this.Name.GetHashCode() ^
                this.PreSpecializationCourseGroup.GetHashCode() ^
                this.MaxRatio.GetHashCode() ^
                this.MinRatio.GetHashCode() ^
                this.Capacity.GetHashCode();
        }
    }
}
