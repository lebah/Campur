using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ImagePathHistory
{
    public class ImagePathHistoryClass
    {
        public string ImagePath { get; set; }
    }

    /// <summary>
    /// Source: https://stackoverflow.com/questions/25368083/using-xml-files-to-store-data-in-c-sharp
    /// </summary>
    public static class XmlHelper
    {
        public static bool NewLineOnAttributes { get; set; }
        /// <summary>
        /// Serializes an object to an XML string, using the specified namespaces.
        /// </summary>
        public static string ToXml(object obj, XmlSerializerNamespaces ns)
        {
            Type T = obj.GetType();

            var xs = new XmlSerializer(T);
            var ws = new XmlWriterSettings { Indent = true, NewLineOnAttributes = NewLineOnAttributes, OmitXmlDeclaration = true };

            var sb = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(sb, ws))
            {
                xs.Serialize(writer, obj, ns);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Serializes an object to an XML string.
        /// </summary>
        public static string ToXml(object obj)
        {
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            return ToXml(obj, ns);
        }

        /// <summary>
        /// Deserializes an object from an XML string.
        /// </summary>
        public static T FromXml<T>(string xml)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            using (StringReader sr = new StringReader(xml))
            {
                return (T)xs.Deserialize(sr);
            }
        }

        /// <summary>
        /// Deserializes an object from an XML string, using the specified type name.
        /// </summary>
        public static object FromXml(string xml, string typeName)
        {
            Type T = Type.GetType(typeName);
            XmlSerializer xs = new XmlSerializer(T);
            using (StringReader sr = new StringReader(xml))
            {
                return xs.Deserialize(sr);
            }
        }

        /// <summary>
        /// Serializes an object to an XML file.
        /// </summary>
        public static void ToXmlFile(Object obj, string filePath)
        {
            var xs = new XmlSerializer(obj.GetType());
            var ns = new XmlSerializerNamespaces();
            var ws = new XmlWriterSettings { Indent = true, NewLineOnAttributes = NewLineOnAttributes, OmitXmlDeclaration = true };
            ns.Add("", "");

            using (XmlWriter writer = XmlWriter.Create(filePath, ws))
            {
                xs.Serialize(writer, obj);
            }
        }

        /// <summary>
        /// Deserializes an object from an XML file.
        /// </summary>
        public static T FromXmlFile<T>(string filePath)
        {
            StreamReader sr = new StreamReader(filePath);
            try
            {
                var result = FromXml<T>(sr.ReadToEnd());
                return result;
            }
            catch (Exception e)
            {
                throw new Exception("There was an error attempting to read the file " + filePath + "\n\n" + e.InnerException.Message);
            }
            finally
            {
                sr.Close();
            }
        }

        public static List<ImagePathHistoryClass> OpenFromXML(string filePath)
        {
            // Check if directory exists
            if (!ValidatePath(filePath))
            {
                CreatePath(AppDomain.CurrentDomain.BaseDirectory + @"\ImagePath");
                CreateFile(AppDomain.CurrentDomain.BaseDirectory + @"\ImagePath\imagePathHistory.xml");
            }
            if (!ValidateFile(filePath))
            {
                CreateFile(AppDomain.CurrentDomain.BaseDirectory + @"\ImagePath\imagePathHistory.xml");
            }

            // Loading XML
            return XmlHelper.FromXmlFile<List<ImagePathHistoryClass>>(filePath);
        }

        public static void CreateFile(string filePath)
        {
            File.Create(filePath);
        }

        public static void CreatePath(string directoryPath)
        {
            Directory.CreateDirectory(directoryPath);
        }

        public static bool SaveToXML(string filePath, ImagePathHistoryClass imagePath)
        {
            try
            {
                /// Check if directory exists
                if (!ValidatePath(filePath))
                {
                    CreatePath(AppDomain.CurrentDomain.BaseDirectory + @"\ImagePath");
                    CreateFile(AppDomain.CurrentDomain.BaseDirectory + @"\ImagePath\imagePathHistory.xml");
                }
                if (!ValidateFile(filePath))
                {
                    CreateFile(AppDomain.CurrentDomain.BaseDirectory + @"\ImagePath\imagePathHistory.xml");
                }

                List<ImagePathHistoryClass> listIph = OpenFromXML(filePath);
                listIph.Add(imagePath);

                XmlHelper.ToXmlFile(listIph, filePath);
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public static bool ValidateFile(string filePath)
        {
            return File.Exists(filePath);
        }

        public static bool ValidatePath(string path)
        {
            return Directory.Exists(path);
        }
    }
}
