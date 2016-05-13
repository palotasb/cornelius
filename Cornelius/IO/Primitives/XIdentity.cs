#pragma warning disable 0649
namespace Cornelius.IO.Primitives
{
    class XIdentity : XBase
    {
        [Map]
        public Semester EffectiveSemester;

        [Map(Required = false)]
        public string Email;

        [Map(Required = false)]
        public string Phone;

        [Map(Required = false)]
        public string Origin;

        public bool Equals(XIdentity identity)
        {
            return
                base.Equals(identity) &&
                this.EffectiveSemester == identity.EffectiveSemester &&
                this.Email == identity.Email &&
                this.Phone == identity.Phone;
        }

        public override bool Equals(object obj)
        {
            if (obj is XIdentity)
            {
                return this.Equals((XIdentity)obj);
            }
            else return false;
        }

        public override int GetHashCode()
        {
            return
                base.GetHashCode() ^
                this.EffectiveSemester.GetHashCode() ^
                (this.Email == null ? 0 : this.Email.GetHashCode()) ^
                (this.Phone == null ? 0 : this.Phone.GetHashCode());
        }
    }
}
