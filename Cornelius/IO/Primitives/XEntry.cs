using System;

#pragma warning disable 0649
namespace Cornelius.IO.Primitives
{
    enum EntryType
    {
        Vizsga,
        Alairas,
        Evkozi,
        Szigorlat,
        Letiltva
    }

    enum EntryValue
    {
        Letiltva = -4000,
        Alairva = -3000,
        Megtagadva = -2000,
        NemTeljesitette = -1000,
        Elegtelen = 1,
        Elegseges = 2,
        Kozepes = 3,
        Jo = 4,
        Jeles = 5
    }

    class XEntry : XBase
    {
        [Map]
        public string Code;

        [Map]
        public string Name;

        [Map]
        public double Credit;

        [Map]
        public EntryType EntryType;

        [Map]
        public EntryValue EntryValue;

        [Map]
        public DateTime EntryDate;

        [Map(Required = false)]
        public EntryType? Requirement;

        [Map(Required = false)]
        public Semester? Semester;

        public bool Equals(XEntry entry)
        {
            return
                base.Equals(entry) &&
                this.Code == entry.Code &&
                this.Credit == entry.Credit &&
                this.EntryType == entry.EntryType &&
                this.EntryValue == entry.EntryValue &&
                this.EntryDate == entry.EntryDate;
        }

        public override bool Equals(object obj)
        {
            if (obj is XEntry)
            {
                return this.Equals((XEntry)obj);
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
                this.Code.GetHashCode() ^
                this.Credit.GetHashCode() ^
                this.EntryType.GetHashCode() ^
                this.EntryValue.GetHashCode() ^
                this.EntryDate.GetHashCode();
        }
    }
}
