using Com.QuantAsylum;
using System;
using System.Collections;
//using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tractor.Com.QuantAsylum.Tractor.TestManagers
{
    /// <summary>
    /// This class is responsible for handling connections to the QA401 and QA450. Most of the 
    /// heavy lifting needs to be done for the QA401 because the QA450 is a REST interface. Once
    /// the QA401 moves to REST interface, this class will mostly be in charge of making sure the
    /// applications are running and can be seen by the Tractor app.
    /// </summary>
    internal static class QAConnectionManager
    {
        public enum Devices { QA401, QA450 };

        static List<string> InstallDirSearchPaths = new List<string>();

        // Bugbug: The first time the user runs the program, we want her to
        // browse to the location of each exe needed by the hardware layer
        const string QA401ExePath = @"QA401\QAAnalyzer.exe";
        const string QA450ExePath = @"QA450.exe";

        static QAConnectionManager()
        {
            // Note the trailing slash
            InstallDirSearchPaths.Add(@"c:\program files\quantasylum\");
            InstallDirSearchPaths.Add(@"c:\program files (x86)\quantasylum\");
            InstallDirSearchPaths.Add(@"d:\program files\quantasylum\");
            InstallDirSearchPaths.Add(@"d:\program files (x86)\quantasylum\");
            InstallDirSearchPaths.Add(@"e:\program files\quantasylum\");
            InstallDirSearchPaths.Add(@"e:\program files (x86)\quantasylum\");

            string fileName = "EXE_Search_Paths.txt";
            if (File.Exists(fileName))
            {
                string[] lines = File.ReadAllLines(fileName);

                foreach (string s in lines)
                {
                    AddSearchPath(s);
                }
            }
        }

        /// <summary>
        /// Adds a search path for the QuantAsylum root install directory
        /// </summary>
        /// <param name="path"></param>
        public static void AddSearchPath(string path)
        {
            // Get rid of any forward slashes and replace with backslash
            path = path.Replace('/', '\\');

            if (path[path.Length - 1] != '\\')
            {
                // Trailing backslash not present, so add it
                path = path + '\\';
            }

            InstallDirSearchPaths.Add(path);
        }

        /// <summary>
        /// Attempts to load assembly from the given name by iterating through all search paths. The 'name'
        /// should specify subdirectory and exe name, for example "QA401\QAAnalyzer.exe" (Note! No leading backslash!)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        static Assembly TryToLoadAssembly(string name)
        {
            Assembly assembly;

            foreach (string dir in InstallDirSearchPaths)
            {
                if (Directory.Exists(dir))
                {
                    try
                    {
                        assembly = Assembly.LoadFrom(dir + name);

                        return assembly;
                    }
                    catch
                    {

                    }
                }
            }

            return null;
        }

        /// <summary>
        /// We only need to connect to QA401. The QA450 doesn't need a connection established
        /// </summary>
        /// <param name="qa401"></param>
        /// <returns></returns>
        public static bool ConnectToDevices(out QA401Interface qa401)
        {
            qa401 = null; 

            try
            {
                // QA401 first
                TcpChannel tcpChannel = (TcpChannel)Helper.GetChannel(4401, false);
                //TcpChannel tcpChannel = new TcpChannel();
                ChannelServices.RegisterChannel(tcpChannel, false);

                Type requiredType = typeof(QA401Interface);

                qa401 = (QA401Interface)Activator.GetObject(requiredType, "tcp://localhost:9401/QuantAsylumQA401Server");

                return true;
            }
            catch
            {

            }

            return false;
        }

        public static bool IsAppRunning(Devices device)
        {
            return IsAppRunning(device, false);
        }

        public static bool IsAppRunning(Devices device, bool remindUser)
        {
            string procName = ""; ;

            switch (device)
            {

                case Devices.QA401:
                    procName = "QAAnalyzer";
                    break;
                case Devices.QA450:
                    procName = "QA450";
                    break;
                default:
                    throw new NotImplementedException("QAConnectionManager IsAppRunning");
            }

            if (Process.GetProcessesByName(procName).Length == 0)
            {
                // In here, the app is not running. Check if the user wants to us to anything else
                if (remindUser)
                    MessageBox.Show("The " + device.ToString() + " application is not running. Please start that application before attempting to control it remotely.");

                return false;
            }

            return true;
        }

        static bool TryToRunExe(string name)
        {
            foreach (string dir in InstallDirSearchPaths)
            {
                string pathToExe = dir + name;
                if (File.Exists(pathToExe))
                {
                    try
                    {
                        ProcessStartInfo psi = new ProcessStartInfo();
                        psi.WorkingDirectory = Path.GetDirectoryName(pathToExe);
                        psi.FileName = pathToExe;
                        System.Diagnostics.Process.Start(psi);

                        // Bugbug: Wait on the named event rather than just sleep
                        Thread.Sleep(1000);
                        return true;
                    }
                    catch
                    {

                    }
                }
            }

            return false;
        }

        public static bool LaunchAppIfNotRunning(Devices device)
        {
            if (IsAppRunning(device))
                return true;

            switch (device)
            {
                case Devices.QA401:
                    return TryToRunExe(QA401ExePath);
                case Devices.QA450:
                    return TryToRunExe(QA450ExePath);
                default:
                    throw new NotImplementedException("QAConnectionManager LaunchAppIfNotRunning");
            }


        }
    }

    /// <summary>
    /// This class is only needed IF we want to connect to multiple devices
    /// </summary>
    public class Helper
    {
        public static IChannel GetChannel(int tcpPort, bool isSecure)
        {
            BinaryServerFormatterSinkProvider serverProv =
                new BinaryServerFormatterSinkProvider();
            serverProv.TypeFilterLevel = TypeFilterLevel.Full;
            IDictionary propBag = new Hashtable();
            propBag["port"] = tcpPort;
            propBag["typeFilterLevel"] = TypeFilterLevel.Full;
            propBag["name"] = Guid.NewGuid().ToString();
            if (isSecure)
            {
                propBag["secure"] = isSecure;
                propBag["impersonate"] = false;
            }
            return new TcpChannel(
                propBag, null, serverProv);
        }
    }
}
