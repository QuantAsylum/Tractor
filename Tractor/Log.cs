using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tractor
{
    enum LogType { General = 0, Http = 0x1, Analyzer = 0x2, Usb = 3, Database = 0x4, Error = 0x4 }

    static class Log
    {
        static StringBuilder Sb = new StringBuilder();

        static Log()
        {
            try
            {
                File.Delete(Constants.LogFile);
            }
            catch (Exception ex)
            {

            }
            DateTime now = DateTime.Now;

            Sb.AppendLine($"Log opened on {now.ToLongDateString()} {now.ToLongTimeString()}");
            Flush();
        }

        static void Flush()
        {
            try
            {
                File.AppendAllText(Constants.LogFile, Sb.ToString());
                Sb = new StringBuilder();
            }
            catch (Exception ex)
            {

            }
        }

        static public void WriteLine(string s)
        {
            WriteLine(LogType.General, s);
        }

        static public void WriteLine(string s, params object[] obj)
        {
            WriteLine(LogType.General, string.Format(s, obj));
        }

        static public void WriteLine(LogType logType, string text, ConsoleColor color = ConsoleColor.White)
        {
            string s = $"{DateTime.Now.ToLongTimeString().PadRight(15)} {text}";
            Sb.AppendLine(s);
            Debug.WriteLine(s);
            Flush();
        }

        static public void Write(string s)
        {
            Write(LogType.General, s);
        }

        static public void Write(LogType logType, string text, ConsoleColor color = ConsoleColor.White)
        {
            string s = $"{DateTime.Now.ToLongTimeString().PadRight(15)} {text}";
            Sb.Append(s);
            Debug.WriteLine(s);
            Flush();
        }
    }
}
