using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cornelius.IO.Mapping;

namespace Cornelius.IO
{
    static class Transform
    {
        public delegate object Cast(string obj);
        private static Dictionary<Type, Cast> Transformations;

        static Transform()
        {
            var dict = Transform.Transformations = new Dictionary<Type,Cast>();
            dict.Add(typeof(string), value => value);
            dict.Add(typeof(Boolean), value => Boolean.Parse(value));
            dict.Add(typeof(DateTime), value => DateTime.Parse(value));
            dict.Add(typeof(Char), value => Char.Parse(value));
            dict.Add(typeof(Byte), value => Byte.Parse(value));
            dict.Add(typeof(SByte), value => SByte.Parse(value));
            dict.Add(typeof(Int16), value => Int16.Parse(value));
            dict.Add(typeof(Int32), value => Int32.Parse(value));
            dict.Add(typeof(Int64), value => Int64.Parse(value));
            dict.Add(typeof(UInt16), value => UInt16.Parse(value));
            dict.Add(typeof(UInt32), value => UInt32.Parse(value));
            dict.Add(typeof(UInt64), value => UInt64.Parse(value));
            dict.Add(typeof(Double), value => Double.Parse(value));
            dict.Add(typeof(Single), value => Single.Parse(value));
        }

        public static Cast ToType(Type type, Dictionary<string, string> translation = null)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                return value => (value == null || value.Length == 0) ? null : Transform.ToType(type.GetGenericArguments()[0], translation)(value);
            }
            else if (Transform.Transformations.ContainsKey(type))
            {
                return Transform.Transformations[type];
            }
            else if (type.IsSubclassOf(typeof(Enum)))
            {
                if (translation != null)
                {
                    return value => Enum.Parse(type, translation[value]);
                }
                else
                {
                    return value => Enum.Parse(type, value);
                }
            }
            else
            {
                foreach (MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.Public).Where(method => method.Name == "op_Implicit" && type.IsAssignableFrom(method.ReturnType)))
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
                    {
                        return value => method.Invoke(null, new object[] { value });
                    }
                }
                return value => value as string; ;
            }
        }

    }
}
