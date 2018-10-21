using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tractor
{
    static class Constants
    {
        public static string TitleBarText = "QuantAsylum TRACTOR";
        public static readonly double Version = 0.64;
        public static string VersionSuffix = "";

        public static string DataFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\QuantAsylum\Tractor\";
        public static string TestLogsPath = DataFilePath + @"\TestLogs\";

        public static int QA450RelaySettle = 700;

    }
}
