using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Vchasno
{
    public static class SerializeToXml
    {
        public static string ToXml<T>(this T value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            XmlSerializerNamespaces xsn = new XmlSerializerNamespaces();
            xsn.Add(String.Empty, String.Empty);

            var xmlserializer = new XmlSerializer(typeof(T));
            var settings = new XmlWriterSettings();
            //settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            //settings.Encoding = Encoding.UTF8;  //Encoding.Unicode;
            var stringWriter = new StringWriter();
            try
            {
                using (var writer = XmlWriter.Create(stringWriter, settings))
                {
                    xmlserializer.Serialize(writer, value, xsn);
                    return stringWriter.ToString();
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        public static T ConvertXml<T>(this string xml)
        {
            var serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(new StringReader(xml));
        }
    }
}

