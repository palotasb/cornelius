using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cornelius
{
    static class Log
    {
        static Log()
        {
            Log.AddConsole();
            Log.AddTarget(new StreamWriter("cornelius.log"));
            Log.Write("Cornelius Szakiránybesoroló Rendszer");
            Log.Write("Copyright 2011-2013 Jánvári Bálint");
            Log.Write();
        }

        private static List<StreamWriter> Targets = new List<StreamWriter>();
        private static Stack<string> Blocks = new Stack<string>();

        public static void AddConsole()
        {
            Log.AddTarget(new StreamWriter(System.Console.OpenStandardOutput(), System.Console.OutputEncoding));
        }

        public static void AddTarget(StreamWriter streamWriter)
        {
            Log.Targets.Add(streamWriter);
        }
        public static void CloseAllTargets()
        {
            foreach (var target in Log.Targets)
            {
                target.Close();
            }
        }

        public static void EnterBlock(string pre = "  ")
        {
            Log.Blocks.Push(pre);
        }

        public static void LeaveBlock()
        {
            Log.Blocks.Pop();
            Log.Write();
        }

        public static void Write(object obj)
        {
            Log.Write(obj.ToString());
        }

        public static void Write(string text = "")
        {
            text = String.Join<string>("", Log.Blocks.Reverse()) + text;
            foreach (var writer in Log.Targets)
            {
                writer.WriteLine(text);
                writer.Flush();
            }
        }
    }
}
