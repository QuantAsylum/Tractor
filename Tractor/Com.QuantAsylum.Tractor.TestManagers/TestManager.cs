using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Com.QuantAsylum.Tractor.Tests;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Serialization.Formatters;
using Tractor.Com.QuantAsylum.Tractor.TestManagers;

namespace Com.QuantAsylum.Tractor.TestManagers
{
    static class TestManager
    {
        /// <summary>
        /// List of all the tests we're going to run
        /// </summary>   
        static public List<TestBase> TestList = new List<TestBase>();

        static public QA401Interface QA401;
        static public IQA450 QA450;

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

        static public void ConnectToDevices()
        {
            QAConnectionManager.LaunchAppIfNotRunning(QAConnectionManager.Devices.QA401);
            QAConnectionManager.LaunchAppIfNotRunning(QAConnectionManager.Devices.QA450);

            if (QAConnectionManager.ConnectToDevices(out QA401, out QA450))
            {

            }
        }

        static public bool AllConnected()
        {
            if (QA401 != null && QA401.IsConnected() && QA450 != null && QA450.IsConnected())
                return true;

            return false;
        }
    }
}
