using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cornelius
{
    /*
     * Egy egyetemi félév
     */
    public struct Semester
    {
        /*
         * A félév belső reprezentációja, 10 * év + félév - 1
         */
        private int _internal;

        /*
         * A félév év komponense
         */
        public int Year
        {
            get
            {
                return _internal / 10;
            }
        }

        /*
         * A félév fél komponense
         */
        public int Half
        {
            get
            {
                return (_internal % 2) + 1;
            }
        }

        /*
         * Félév létrehozása a két komponensből
         */
        public Semester(int year, int half)
        {
            _internal = year * 10 + (half - 1) % 2;
        }

        /*
         * Értékadás engedélyezése szövegből. Megkísérli felismerni a formátumát,
         * egy elég favágó, de működő módszerrel.
         */
        public static implicit operator Semester(string text)
        {
            if (text == null || text.Length == 0)
            {
                throw new InvalidCastException("Semester cannot be recovered from string - empty or null string.");
            }
            if (text.Length == 11 || text.Length == 9)
            {
                // 2011/2012/1 vagy 2011/12/1
                return new Semester(Int32.Parse(text.Substring(0, 4)), Int32.Parse(text.Substring(text.Length - 1, 1)));
            }
            else if (text.Length == 10)
            {
                // Dátum, pl. 2011.02.03 vagy 2011/02/03
                int year = Int32.Parse(text.Substring(0, 4));
                int month = Int32.Parse(text.Substring(5, 2));
                if (month <= 7)
                {
                    return new Semester(year - 1, 2);
                }
                else
                {
                    return new Semester(year, 1);
                }
            }
            else if (text.Length == 6)
            {
                // Egyszerű formátum 2011/1
                return new Semester(Int32.Parse(text.Substring(0, 4)), Int32.Parse(text.Substring(5, 1)));
            }
            else
            {
                throw new InvalidCastException("Semester cannot be recovered from string - unknown format.");
            }
        }

        /*
         * 2011/2012/1 formátumban adja vissza
         */
        public override string ToString()
        {
            return this.Year.ToString() + "/" + (this.Year + 1).ToString() + "/" + this.Half;
        }

        /*
         * Ellenőrzi, hogy a megadott határok között van-e a félév
         */
        public static bool InInterval(Semester? semester, Semester? from, Semester? to)
        {
            return semester != null && (from == null || semester >= from) && (to == null || semester <= to);
        }

        /*
         * A dátum alapján kitalálja, melyik félév.
         */
        public static Semester FromDate(DateTime date)
        {
            return new Semester(date.Month >= 8 ? date.Year : date.Year - 1, date.Month >= 8 || date.Month < 2 ? 1 : 2);
        }
        
        #region Egyenlőség és összehasonlítás

        public static bool operator <(Semester first, Semester second)
        {
            return first._internal < second._internal;
        }

        public static bool operator >(Semester first, Semester second)
        {
            return first._internal > second._internal;
        }

        public static bool operator <=(Semester first, Semester second)
        {
            return first._internal <= second._internal;
        }

        public static bool operator >=(Semester first, Semester second)
        {
            return first._internal >= second._internal;
        }

        public static bool operator ==(Semester first, Semester second)
        {
            return first._internal == second._internal;
        }

        public static bool operator !=(Semester first, Semester second)
        {
            return first._internal != second._internal;
        }

        public override bool Equals(object obj)
        {
            if (obj is Semester)
            {
                return (Semester)obj == this;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return _internal;
        }

        #endregion
    }
}
