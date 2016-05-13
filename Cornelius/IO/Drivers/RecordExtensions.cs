using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cornelius.IO.Drivers
{
    static class RecordExtensions
    {
        public static IEnumerable<string> AsEnumerable(this IRecord record)
        {
            for (int i = 0; i < record.Count; ++i)
            {
                yield return record[i];
            }
        }

        public static IEnumerable<string> AsEnumerable(this Record record)
        {
            return record.Enumerable;
        }

        public static int IndexOf(this IRecord record, string element)
        {
            for (int i = 0; i < record.Count; ++i)
            {
                if (record[i] == element)
                {
                    return i;
                }
            }
            return -1;
        }

        public static IRecord AsRecord(this IEnumerable<string> elements)
        {
            return new Record(elements);
        }
    }
}
