using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tractor
{
    enum LogType { General = 0, Http = 0x1, Analyzer = 0x2, Usb = 3, Error = 0x4 }

    static class Log
    {
        static public void WriteLine(LogType logType, string text)
        {
            Debug.WriteLine(text);
        }
    }
}
