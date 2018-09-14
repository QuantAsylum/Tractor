using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Com.QuantAsylum.Tractor.Settings 
{
    /// <summary>
    /// Saves and retrieves app settings to an xml file. This requires class and members to be public
    /// </summary>
    static class SerDes
    {
        static public string Serialize(object obj)
        {
            // This gets rids of the xlms decorations at the top of the XML file
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            XmlSerializer serializer = new XmlSerializer(obj.GetType());
            using (StringWriter stream = new StringWriter())
            {
                serializer.Serialize(stream, obj, ns);
                stream.Flush();
                string s = stream.ToString();
                return s;
            }
        }

        static public object Deserialize(Type t, string xml)
        {
            object result;

            if (string.IsNullOrEmpty(xml))
            {
                throw new ArgumentNullException("xml deserialization failed in SerDes.Deserialize");
            }

            var serializer = new XmlSerializer(t);

            using (TextReader reader = new StringReader(xml))
            {
                result = serializer.Deserialize(reader);
            }

            return result;
        }
    }
}
