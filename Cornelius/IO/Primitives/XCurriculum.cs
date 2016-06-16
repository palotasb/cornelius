using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#pragma warning disable 0649
namespace Cornelius.IO.Primitives
{
    enum Curriculum
    {
        None,
        Until2013,
        From2014
    }

    class XCurriculum
    {
        [Map]
        public Curriculum Curriculum;

        [Map]
        public string EducationProgramPostfix;

        public XCurriculum()
        {
            EducationProgramPostfix = "";
        }

        public override bool Equals(object obj)
        {
            if (obj is XCurriculum)
            {
                return Equals((XCurriculum)obj);
            }
            else
            {
                return false;
            }
        }

        public bool Equals(XCurriculum obj)
        {
            return
                Curriculum == obj.Curriculum &&
                EducationProgramPostfix == obj.EducationProgramPostfix;
        }

        public override int GetHashCode()
        {
            return
                Curriculum.GetHashCode() ^
                EducationProgramPostfix.GetHashCode();
        }
    }
}
