using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cornelius.IO.Drivers;

namespace Cornelius.IO
{
    class TableReader
    {
        public static IEnumerable<T> Read<T>(string path)
            where T : new()
        {
            return TableReader.Read<T>(path, Driver.GetInputDriverForFile(path));
        }

        public static IEnumerable<T> Read<T>(string path, IInputDriver driver)
            where T : new()
        {
            var table = (TableAttribute)typeof(T).GetCustomAttributes(typeof(TableAttribute), false).First();
            var fields = typeof(T).GetFields().Where(field => field.GetCustomAttributes(typeof(ColumnAttribute), true).Length > 0).OrderBy(field => ((ColumnAttribute)field.GetCustomAttributes(typeof(ColumnAttribute), true).First()).Number).ToArray();
            var reader = driver.GetReader(path, table.Name);

            foreach (var row in reader.Skip(1))
            {
                T item = new T();
                for (int i = 0; i < fields.Length; ++i)
                {
                    fields[i].SetValue(item, Transform.ToType(fields[i].FieldType)(row[i]));
                }
                yield return item;
            }

            reader.Dispose();
        }
    }
}
