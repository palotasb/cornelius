using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cornelius.IO
{
    static class Provision
    {
        private static Dictionary<Type, int> Indexes = new Dictionary<Type, int>();

        public static int UniqueId (object obj)
        {
            Type type = obj.GetType();
            if (Provision.Indexes.ContainsKey(type))
            {
                Provision.Indexes[type]++;
                return Provision.Indexes[type];
            }
            else
            {
                Provision.Indexes.Add(type, 1);
                return 1;
            }
        }

        public static void Clear()
        {
            Provision.Indexes.Clear();
        }

        public static void ContinueFrom(Type type, int index)
        {
            if (Provision.Indexes.ContainsKey(type))
            {
                Provision.Indexes[type] = index;
            }
            else
            {
                Provision.Indexes.Add(type, index);
            }
        }
    }
}
