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

        public string TestClass = "";

        public bool AbortOnFailure;

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
