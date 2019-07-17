using Com.QuantAsylum.Tractor.Tests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Com.QuantAsylum.Tractor.Settings
{
    public class AppSettings
    {
        public List<TestBase> TestList = new List<TestBase>();

        public string TestClass = "Com.QuantAsylum.Tractor.TestManagers.QA401";

        public bool AbortOnFailure = true;

        public string DbConnectString = "Server=MyPc\\SQLEXPRESS;Integrated security = SSPI; Initial Catalog = QATestDB; User ID = sa; Password=password";

        public bool UseDb = false;

        public string DbSessionName = "";

        public bool UseAuditDb = false;

        public Guid ProductId = Guid.NewGuid();

        public string AuditDbSessionName = "";

        public string AuditDbEmail = "youremail@yourcompany.com";

        public bool LockTestScreen = false;

        public string Password = "";

        /// <summary>
        /// Finds a unique name in the TestList given a root. For example, if
        /// the root is "THD" and the list is empty, then "THD-1" will be returned, 
        /// and then "THD-2" will be returned, etc. 
        /// </summary>
        /// <param name="nameRoot"></param>
        /// <returns></returns>
        public string FindUniqueName(string nameRoot)
        {
            nameRoot += "-";
            string newName = "";

            // Bounded search for unique names
            for (int i = 0; i < 10000; i++)
            {
                newName = nameRoot + i.ToString();

                if (TestList.Count == 0)
                    return newName;

                try
                {
                    if (TestList.First(o => o.Name == newName) != null)
                    {
                        // This name is already being used. Keep going
                    }
                }
                catch
                {
                    // Nothing matched. We found our unique name
                    break;
                }
            }

            // Here, we've found a unique name
            return newName;
        }

        public string Serialize()
        {
            XmlSerializer serializer = new XmlSerializer(this.GetType());
            using (StringWriter stream = new StringWriter())
            {
                serializer.Serialize(stream, this);
                stream.Flush();
                string s = stream.ToString();
                return s;
            }
        }

        public AppSettings Deserialize(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                throw new ArgumentNullException("xml deserialization failed in Settings");
            }

            XmlSerializer serializer = new XmlSerializer(typeof(AppSettings));
            using (StringReader stream = new StringReader(xml))
            {
                try
                {
                    return (AppSettings)serializer.Deserialize(stream);
                }
                catch (Exception ex)
                {
                    // The serialization error messages are cryptic at best.
                    // Give a hint at what happened
                    throw new InvalidOperationException("Failed to create object from xml string in Settings", ex);
                }
            }
        }
    }
}
