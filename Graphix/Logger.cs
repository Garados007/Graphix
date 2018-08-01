using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Graphix
{
    public static class Logger
    {
        static string nl = Environment.NewLine;

        public static void Log(string text)
        {
            var frame = new System.Diagnostics.StackTrace().GetFrame(1);
            var method = frame.GetMethod();
            write(string.Format("[{0}] {2}.{3}() [{4}({5},{6})]: {1}", DateTime.Now.ToString("dd.MM.yyyy HH:MM:ss"), text, 
                method.ReflectedType.FullName, method.Name,
                frame.GetFileName(), frame.GetFileLineNumber(), frame.GetFileColumnNumber()));
        }

        public static void Error(Exception e)
        {
            var trace = e.StackTrace.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            Log(string.Format("{0}{3}\tin: {1}{3}\tTrace:{3}\t\t{2}",
                e.Message, e.Source,
                string.Join(nl+"\t\t", e.StackTrace.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)), nl));
        }

        static void write(string text)
        {
            File.AppendAllText("log.txt", text + nl);
        }
    }
}
