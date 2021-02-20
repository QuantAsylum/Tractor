using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tractor
{
    static class Constants 
    {
        public static string TitleBarText = "QuantAsylum TRACTOR";
        public static readonly double Version = 1.01;
        public static string VersionSuffix = "";

        public static double RequiredWebserviceVersion = 0.5;
        public static double RequiredQa401Version = 1.923;
        public static double RequiredQa450Version = 1.21;
        public static double RequiredQa351Version = 1.033;

        public static string DataFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "QuantAsylum", "Tractor");
        public static string MaskFiles = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "QuantAsylum", "QA401", "UserMasks");
        static public string DefaultSettingsFile = Path.Combine(DataFilePath, "Default.Settings");
        public static string TestLogsPath = Path.Combine(DataFilePath, "TestLogs");
        public static string CsvLogsPath = Path.Combine(DataFilePath, "CsvLogs");
        public static string LogFileName = "index.html";
        public static string AuditPath = Path.Combine(DataFilePath, "AuditData");
        public static string PidPath = Path.Combine(DataFilePath, "ProductIds");
        static public string LogFile = Path.Combine(DataFilePath, "Tractor_Log.txt");
        static public string TmpLogFile = Path.Combine(DataFilePath, "Tractor_Log_tmp.txt");
        static public string AutoDocFile = Path.Combine(DataFilePath, "Tractor_Help.md");
    }
}
