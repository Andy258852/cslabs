using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;

namespace Configurator
{
    public static class OptionManager
    {
        public static string DefaultXmlPath { get; set; } = "config.xml";
        public static string DefaultXsdPath { get; set; } = "config.xsd";
        public static string DefaultJsonPath { get; set; } = "config.json";
        public static string DefaultJsonSchemaPath { get; set; } = "config.jschema";
        public static string LogPath { get; set; } = "log.txt";
        public static bool ValidatingWithSchema { get; set; } = true;
        public static bool LogEnabled { get; set; } = true;
        public static bool XmlIsHigher { get; set; } = true;

        static OptionManager()
        {
            if (!File.Exists(LogPath))
                File.Create(LogPath);
        }

        public static void ChangeDir(string path)
        {
            DefaultXmlPath = Path.Combine(path, "config.xml");
            DefaultXsdPath = Path.Combine(path, "config.xsd");
            DefaultJsonPath = Path.Combine(path, "config.json");
            DefaultJsonSchemaPath = Path.Combine(path, "config.jschema");
            LogPath = Path.Combine(path, "log.txt");
        }

        static void CheckPath(string path)
        {
            if (File.Exists(path))
                return;
            else
            {
                Log($"File {path} wasn't found.");
                throw new Exception($"File {path} wasn't found.");
            }
        }

        public static object GetOption(Option option)
        {
            return GetOptions(option)[0];
        }

        public static object[] GetOptions(params Option[] options)
        {

            bool a, b;
            if (ValidatingWithSchema)
            {
                a = XmlIsValid();
                b = JsonIsValid();
                if (a && !b)
                {
                    ReadXml(out object[] res, out int count, options);
                    return res;
                }
                else if (!a && b)
                {
                    ReadJson(out object[] res, out int count, options);
                    return res;
                }
                else if (!a && !b)
                {
                    Log("All config files have incorrect formats");
                    throw new Exception("Incorrect formats");
                }
            }

            int count1 = 0, count2 = 0;
            object[] res1 = null, res2 = null;

            try
            {
                ReadXml(out res1, out count1, options);
                a = true;
            }
            catch
            {
                a = false;
            }

            try
            {
                ReadJson(out res2, out count2, options);
                b = true;
            }
            catch (Exception ex)
            {
                b = false;
                if (!a && !b)
                {
                    throw new Exception(ex.Message);
                }
            }

            if (a && !b)
            {
                return res1;
            }
            else if (!a && b)
            {
                return res2;
            }
            else
            {
                return count1 > count2 ? res1 : count2 != count1 ? res2 : XmlIsHigher ? res1 : res2;
            }
        }

        static bool JsonIsValid()
        {
            CheckPath(DefaultJsonPath);
            CheckPath(DefaultJsonSchemaPath);

            JSchema schema = JSchema.Parse(File.ReadAllText(DefaultJsonSchemaPath));
            var el = JObject.Parse(File.ReadAllText(DefaultJsonSchemaPath));
            bool result = el.IsValid(schema, out IList<string> msg);
            foreach (var temp in msg)
            {
                Log($"Incorrect format while validating: {temp}");
            }
            return result;
        }

        static bool XmlIsValid()
        {
            CheckPath(DefaultXmlPath);
            CheckPath(DefaultXsdPath);

            try
            {
                XmlSchemaSet schema = new XmlSchemaSet();
                schema.Add("", DefaultXsdPath);
                XmlReader rd = XmlReader.Create(DefaultXmlPath);
                XDocument doc = XDocument.Load(rd);
                void ValidationEventHandler(object sender, System.Xml.Schema.ValidationEventArgs e)
                {
                    if (Enum.TryParse<XmlSeverityType>("Error", out XmlSeverityType type))
                    {
                        if (type == XmlSeverityType.Error)
                        {
                            Log($"Incorrect format while validating: {e.Message}");
                            throw new Exception(e.Message);
                        }
                    }
                }
                doc.Validate(schema, ValidationEventHandler);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void Log(string message)
        {
            if (LogEnabled)
                File.AppendAllText(LogPath, message + '\n');
        }

        static void ReadJson(out object[] values, out int count, params Option[] options)
        {
            if (!ValidatingWithSchema)
                CheckPath(DefaultJsonPath);

            var JsonString = File.ReadAllText(DefaultJsonPath);
            JsonValue json;
            try
            {
                json = JsonValue.Parse(JsonString);
            }
            catch (Exception ex)
            {
                Log($"Incorrect Json Format: {ex.Message}");
                throw new Exception(ex.Message);
            }

            count = 0;
            values = new object[options.Length];
            ReadJsonFrom(json, ref values, options);

            for (int i = 0; i < options.Length; i++)
            {
                if (values[i] == null)
                {
                    if (options[i].IsNecessary)
                    {
                        Log($"Necessary option {options[i].Name} wasn't found.");
                        throw new Exception($"Necessary option {options[i].Name} wasn't found.");
                    }
                    else
                    {
                        values[i] = options[i].DefaultValue;
                    }
                }
                else
                {
                    count++;
                }
            }
        }

        static void ReadJsonFrom(JsonValue json, ref object[] values, params Option[] options)
        {
            for (int i = 0; i < options.Length; i++)
            {
                if (json.ContainsKey(options[i].Name))
                {
                    TryToConvert(options[i].CType, (string)json[options[i].Name], ref values[i]);
                }
            }
        }

        static void ReadXml(out object[] values, out int count, params Option[] options)
        {
            if (!ValidatingWithSchema)
                CheckPath(DefaultXmlPath);

            XmlDocument document = new XmlDocument();
            try
            {
                document.Load(DefaultXmlPath);
            }
            catch (Exception ex)
            {
                Log($"Incorrect Xml Format: {ex.Message}");
                throw new Exception(ex.Message);
            }
            count = 0;
            XmlElement xRoot = document.DocumentElement;
            values = new object[options.Length];
            ReadXmlFrom(xRoot, ref values, options);
            for (int i = 0; i < options.Length; i++)
            {
                if (values[i] == null)
                {
                    if (options[i].IsNecessary)
                    {
                        Log($"Necessary option {options[i].Name} wasn't found.");
                        throw new Exception($"Necessary option {options[i].Name} wasn't found.");
                    }
                    else
                    {
                        values[i] = options[i].DefaultValue;
                    }
                }
                else
                {
                    count++;
                }
            }
        }

        static void ReadXmlFrom(XmlNode xRoot, ref object[] values, Option[] options)
        {
            foreach (XmlNode xmlNode in xRoot.ChildNodes)
            {
                for (int j = 0; j < options.Length; j++)
                {
                    if (options[j].Name == xmlNode.Name)
                    {
                        for (int i = 0; i < xmlNode.ChildNodes.Count; i++)
                        {
                            if (xmlNode.ChildNodes[i].NodeType == XmlNodeType.Text)
                            {
                                TryToConvert(options[j].CType, xmlNode.ChildNodes[i].Value, ref values[j]);
                                break;
                            }
                        }
                    }
                }

                if (xmlNode.Attributes != null)
                {
                    for (int i = 0; i < options.Length; i++)
                    {
                        if (xmlNode.Attributes[options[i].Name] != null)
                        {
                            TryToConvert(options[i].CType, xmlNode.Attributes[options[i].Name].InnerText, ref values[i]);
                            break;
                        }
                    }
                }
                if (xmlNode.HasChildNodes)
                    ReadXmlFrom(xmlNode, ref values, options);
            }
        }

        static void TryToConvert(Type type, string value, ref object result)
        {
            try
            {
                if (type == typeof(string))
                {
                    result = value.Trim(' ', '\"');
                }
                else
                {
                    Convert.ChangeType(value, type);
                    result = value;
                }
            }
            catch
            {
                Log($"Conversion of {value} to {type} was failed.");
            }
        }
    }
}
