using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Com.QuantAsylum.Tractor.Tests;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;

namespace Com.QuantAsylum.Tractor.TestManagers
{
    static class TestManager
    {
        /// <summary>
        /// List of all the tests we're going to run
        /// </summary>   
        static public List<TestBase> TestList = new List<TestBase>();

        static public QA401Interface AudioAnalyzer;

        static public string FindUniqueName(string nameRoot)
        {
            nameRoot += "-";
            string newName = "";

            for (int i = 0; i < 10000; i++)
            {
                newName = nameRoot + i.ToString();

                bool nameIsUnique = true;

                foreach (TestBase Test in TestList)
                {
                    if (Test.Name == newName)
                    {
                        nameIsUnique = false;
                        break;
                    }
                }

                if (nameIsUnique)
                    break;
            }

            // Here, we've found a unique name
            return newName;
        }

        static public void ConnectQA401()
        {
            // Try to connect to QA400 Application. Note the code below is boilerplate, likely needed by any app that wants to connect
            // to the QA400 application. This is routine dotnet remoting code. 
            try
            {
                TcpChannel tcpChannel = new TcpChannel();
                ChannelServices.RegisterChannel(tcpChannel, false);

                Type requiredType = typeof(QA401Interface);

                AudioAnalyzer = (QA401Interface)Activator.GetObject(requiredType, "tcp://localhost:9401/QuantAsylumQA401Server");

                if (AudioAnalyzer.IsConnected() == false)
                    AudioAnalyzer = null;
            }
            catch
            {
                // If the above fails for any reason, make sure the rest of the app can tell. We do that here by setting AudioAnalyzer to null.
                AudioAnalyzer = null;
            }
        }
    }
}
