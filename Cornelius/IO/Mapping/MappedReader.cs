using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Cornelius.IO.Drivers;

namespace Cornelius.IO.Mapping
{
    static class MappedReader
    {
        public static IEnumerable<T> Read<T>(string path, Map map)
            where T : new()
        {
            return MappedReader.Read<T>(path, map, Driver.GetInputDriverForFile(path));
        }

        public static IEnumerable<T> Read<T>(string path,  Map map, IInputDriver driver)
            where T : new()
        {
            var reader = driver.GetReader(path, null);
            List<Assignment> assignments = new List<Assignment>();
            FieldInfo[] fields = typeof(T).GetFields();

            foreach (FieldInfo field in fields)
            {
                MapAttribute attribute = (MapAttribute)field.GetCustomAttributes(typeof(MapAttribute), false).FirstOrDefault();
                if (attribute != null)
                {
                    Assignment assignment = null;
                    var entries = map.Columns.Where(e => e.Value == field.Name);
                    foreach (var entry in entries)
                    {
                        int index = reader.FirstRow.IndexOf(entry.Key);
                        if (index >= 0)
                        {
                            assignment = new Assignment(index, field);
                            break;
                        }
                    }

                    if (assignment == null && attribute.Required)
                    {
                        throw new Exception("A \"" + field.Name + "\" mező nem található a térképben vagy a fájlban.");
                    }
                    else if (assignment != null)
                    {
                        Translation translation = null;
                        if (map.Enums.ContainsKey(field.Name))
                        {
                            translation = map.Enums[field.Name];
                        }
                        assignment.Cast = Transform.ToType(field.FieldType, translation);
                        assignments.Add(assignment);
                    }
                }
            }

            foreach (var record in reader.Skip(1))
            {
                T obj = new T();
                foreach (var assignment in assignments)
                {
                    assignment.Field.SetValue(obj, assignment.Cast(record[assignment.Index]));
                }
                yield return obj;
            }

            reader.Dispose();
        }
    }
}
