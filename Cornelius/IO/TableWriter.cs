using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cornelius.IO.Drivers;

namespace Cornelius.IO
{
    static class TableWriter
    {
        public static void Write<T>(string path, IEnumerable<T> rows, IOutputDriver driver)
        { 
            var table = (TableAttribute)typeof(T).GetCustomAttributes(typeof(TableAttribute), false).First();
            Write(path, rows, driver, table.Name);
        }

        public static void Write<T>(string path, IEnumerable<T> rows, IOutputDriver driver, string name)
        {
            var fields = typeof(T).GetFields().Where(field => field.GetCustomAttributes(typeof(ColumnAttribute), true).Length > 0).ToDictionary(field => (ColumnAttribute)field.GetCustomAttributes(typeof(ColumnAttribute), true).First());
            var writer = driver.GetWriter(path, name, fields.OrderBy(field => field.Key.Number).Select(field => field.Key.Name));

            if (fields.Any(pair => pair.Key.Sort))
            {
                rows = rows.OrderBy(row => fields.First(pair => pair.Key.Sort).Value.GetValue(row));
            }

            foreach (var row in rows)
            {
                writer.WriteRecord(fields.OrderBy(pair => pair.Key.Number).Select(pair => pair.Value.GetValue(row)).Select(cell => cell == null ? null : cell.ToString()).AsRecord());
            }
            writer.SaveFile();
        }
    }
}
