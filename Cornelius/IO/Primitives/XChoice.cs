#pragma warning disable 0649
namespace Cornelius.IO.Primitives
{
    class XChoice : XBase
    {
        [Map]
        public int Number;

        [Map]
        public string Name;

        public bool Equals(XChoice choice)
        {
            return
                base.Equals(choice) &&
                this.Number == choice.Number &&
                this.Name == choice.Name;
        }

        public override bool Equals(object obj)
        {
            if (obj is XChoice)
            {
                return this.Equals((XChoice)obj);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return
                base.GetHashCode() ^
                this.Number.GetHashCode() ^ 
                this.Name.GetHashCode();
        }
    }
}
