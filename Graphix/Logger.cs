using System;
using System.IO;

namespace Graphix
{
    /// <summary>
    /// Logs all messages of the core to the logfile
    /// </summary>
    public static class Logger
    {
        static string nl = Environment.NewLine;

        /// <summary>
        /// log a single line of text to the log file
        /// </summary>
        /// <param name="text">text to log</param>
        public static void Log(string text)
        {
            var frame = new System.Diagnostics.StackTrace().GetFrame(1);
            var method = frame.GetMethod();
            write(string.Format("[{0}] {2}.{3}() [{4}({5},{6})]: {1}", DateTime.Now.ToString("dd.MM.yyyy HH:MM:ss"), text, 
                method.ReflectedType.FullName, method.Name,
                frame.GetFileName(), frame.GetFileLineNumber(), frame.GetFileColumnNumber()));
        }

        /// <summary>
        /// log an exception to the log file
        /// </summary>
        /// <param name="e">the exception object</param>
        public static void Error(Exception e)
        {
            var trace = e.StackTrace.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            Log(string.Format("{0}{3}\tin: {1}{3}\tTrace:{3}\t\t{2}",
                e.Message, e.Source,
                string.Join(nl+"\t\t", e.StackTrace.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)), nl));
        }

        /// <summary>
        /// write a string to the log file
        /// </summary>
        /// <param name="text">text to write</param>
        static void write(string text)
        {
            File.AppendAllText("log.txt", text + nl);
        }
    }
}
