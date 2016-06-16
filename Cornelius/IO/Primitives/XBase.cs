#pragma warning disable 0649
namespace Cornelius.IO.Primitives
{
    class XBase
    {
        public string Key
        {
            get
            {
                return this.BaseEducationProgram + " / " + this.BaseNeptun;
            }
        }

        [Map]
        public string BaseNeptun;

        [Map]
        public string BaseEducationProgram;

        [Map(Required = false)]
        public string BaseName;

        [Map(Required = false)]
        public Curriculum? Curriculum;

        public override bool Equals(object obj)
        {
            if (obj is XBase)
            {
                return this.Equals((XBase)obj);
            }
            else
            {
                return false;
            }
        }

        public bool Equals(XBase obj)
        {
            return
                this.BaseNeptun == obj.BaseNeptun &&
                this.BaseEducationProgram == obj.BaseEducationProgram &&
                this.BaseName == obj.BaseName &&
                this.Curriculum == obj.Curriculum;
        }

        public override int GetHashCode()
        {
            return this.BaseNeptun.GetHashCode() ^
                (this.BaseEducationProgram == null ? 0 : this.BaseEducationProgram.GetHashCode()) ^
                (this.BaseName == null ? 0 : this.BaseName.GetHashCode()) ^
                (this.Curriculum == null ? 0 : this.Curriculum.GetHashCode());
        }
    }
}
