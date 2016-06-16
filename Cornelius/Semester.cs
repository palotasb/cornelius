using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cornelius
{
    /// <summary>
    /// Egy egyetemi félévet reprezentáló struktúra.
    /// </summary>
    public struct Semester
    {
        /// <summary>
        /// A félév belső reprezentációja, 10 * év + félév - 1
        /// </summary>
        [System.Diagnostics.DebuggerDisplay("{ToString()}")]
        private int _internal;

        /// <summary>
        /// A félév év komponense.
        /// </summary>
        public int Year
        {
            get
            {
                return _internal / 10;
            }
        }

        /// <summary>
        /// A félév fél komponense
        /// </summary>
        public int Half
        {
            get
            {
                return (_internal % 2) + 1;
            }
        }

        /// <summary>
        /// Félév létrehozása a két komponensből
        /// </summary>
        /// <param name="year">Az év.</param>
        /// <param name="half">A félév (1 vagy 2)</param>
        public Semester(int year, int half)
        {
            _internal = year * 10 + (half - 1) % 2;
        }

        /// <summary>
        /// Értékadás engedélyezése szövegből. Megkísérli felismerni a formátumát,
        /// egy elég favágó, de működő módszerrel.
        /// </summary>
        /// <param name="text">A félév leírását tartalmazó string</param>
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

        /// <summary>
        /// 2011/2012/1 formátumban adja vissza a félévet.
        /// </summary>
        /// <returns>A félév szöveges leírása.</returns>
        public override string ToString()
        {
            return this.Year.ToString() + "/" + (this.Year + 1).ToString() + "/" + this.Half;
        }

        /// <summary>
        /// Ellenőrzi, hogy a megadott határok között van-e a félév (inkluzív)
        /// </summary>
        /// <param name="semester">Félévet reprezentáló struktúra.</param>
        /// <param name="from">Összehasonlítási alap (-tól)</param>
        /// <param name="to">Összehasonlítási alap (-ig)</param>
        /// <returns></returns>
        public static bool InInterval(Semester? semester, Semester? from, Semester? to)
        {
            return semester != null && (from == null || semester >= from) && (to == null || semester <= to);
        }

        /// <summary>
        /// Dátum objektumból készít félév struktúrát.
        /// </summary>
        /// <param name="date">A félévbe eső dátum.</param>
        /// <returns></returns>
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
