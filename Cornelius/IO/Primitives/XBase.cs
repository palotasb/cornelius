#pragma warning disable 0649
namespace Cornelius.IO.Primitives
{
    class XBase
    {
        public string Key
        {
            get
            {
                return this.BaseGroup + " / " + this.BaseNeptun;
            }
        }

        [Map]
        public string BaseNeptun;

        [Map]
        public string BaseGroup;

        [Map(Required = false)]
        public string BaseName;

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
                this.BaseGroup == obj.BaseGroup &&
                this.BaseName == obj.BaseName;
        }

        public override int GetHashCode()
        {
            return this.BaseNeptun.GetHashCode() ^
                (this.BaseGroup == null ? 0 : this.BaseGroup.GetHashCode()) ^
                (this.BaseName == null ? 0 : this.BaseName.GetHashCode());
        }
    }
}
