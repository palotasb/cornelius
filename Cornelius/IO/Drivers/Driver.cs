using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cornelius.IO.Drivers
{
    static class Driver
    {
        private static Dictionary<string, IInputDriver> InputDrivers;
        private static Dictionary<string, IOutputDriver> OutputDrivers;

        static Driver()
        {
            Driver.InputDrivers = new Dictionary<string, IInputDriver>();
            Driver.OutputDrivers = new Dictionary<string, IOutputDriver>();

            var readerInterface = typeof(IDriver);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsInterface && readerInterface.IsAssignableFrom(type));

            foreach (var type in types)
            {
                IDriver driver = (IDriver)type.GetConstructor(new Type[] { }).Invoke(new object[] { });
                foreach (var extension in driver.Extensions)
                {
                    if (driver is IInputDriver)
                    {
                        Driver.InputDrivers.Add(extension.ToLower(), driver as IInputDriver);
                    }
                    if (driver is IOutputDriver)
                    {
                        Driver.OutputDrivers.Add(extension.ToLower(), driver as IOutputDriver);
                    }
                }
            }
        }

        private static string GetExtension(string path)
        {
            return Path.GetExtension(path).ToLower().Substring(1);
        }

        public static bool CanReadFile(string path)
        {
            return Driver.OutputDrivers.ContainsKey(Driver.GetExtension(path));
        }

        public static bool CanWriteFile(string path)
        {
            return Driver.InputDrivers.ContainsKey(Driver.GetExtension(path));
        }

        public static IInputDriver GetInputDriverForFile(string path)
        {
            return Driver.InputDrivers[Driver.GetExtension(path)];
        }

        public static IOutputDriver GetOutputDriverForFile(string path)
        {
            return Driver.OutputDrivers[Driver.GetExtension(path)];
        }
    }
}
