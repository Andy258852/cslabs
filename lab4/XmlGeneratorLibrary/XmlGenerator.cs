using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace XmlGeneratorLibrary
{
    public class XmlGenerator : IXmlGenerator
    {
        string Path { get; set; }

        public XmlGenerator(string path)
        {
            Path = path;
        }

        public string GetXmlString(object input)
        {
            XmlSerializer ser = new XmlSerializer(input.GetType());
            string res = null;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                ser.Serialize(memoryStream, input);
                memoryStream.Position = 0;
                res = new StreamReader(memoryStream).ReadToEnd();
            }
            return res;
        }

        public XmlDocument GetXmlDocument(object input)
        {
            XmlSerializer ser = new XmlSerializer(input.GetType());
            XmlDocument xd = null;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                ser.Serialize(memoryStream, input);
                memoryStream.Position = 0;
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreWhitespace = true;
                using (var xtr = XmlReader.Create(memoryStream, settings))
                {
                    xd = new XmlDocument();
                    xd.Load(xtr);
                }
            }
            return xd;
        }

        public void SaveXml(object input, string filename)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(input.GetType());
            FileStream fileStream = File.Create(System.IO.Path.Combine(Path, filename));
            xmlSerializer.Serialize(fileStream, input);
            fileStream.Close();
        }

        public string GetXsd(string xmlContent)
        {
            XmlReader reader = XmlReader.Create(new StringReader(xmlContent));
            XmlSchemaSet schemaSet = new XmlSchemaSet();
            XmlSchemaInference schema = new XmlSchemaInference();
            schemaSet = schema.InferSchema(reader);
            string result = "";
            foreach (XmlSchema s in schemaSet.Schemas())
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    s.Write(memoryStream);
                    memoryStream.Position = 0;
                    result = new StreamReader(memoryStream).ReadToEnd();
                }
            }
            return result;
        }

        public void SaveXsd(string xmlContent, string filename)
        {
            using (StreamWriter sw = File.CreateText(System.IO.Path.Combine(Path, filename)))
            {
                sw.Write(GetXsd(xmlContent));
            }
        }
    }
}
