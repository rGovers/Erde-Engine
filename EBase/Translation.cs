using Erde.IO;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Erde
{
    public static class Translation
    {
        static Dictionary<string, string> FontTable;
        static Dictionary<string, string> TranslationTable;

        static void AddLocalization(XmlNode a_parentNode)
        {
            for (XmlNode node = a_parentNode.FirstChild; node != null; node = node.NextSibling)
            {
                AddTranslationTag(node.Name, node.InnerText);
            }
        }
        static void AddFontTable(XmlNode a_parentNode)
        {
            for (XmlNode node = a_parentNode.FirstChild; node != null; node = node.NextSibling)
            {
                AddFontTag(node.Name, node.InnerText);
            }
        }

        public static void LoadLocalization(string a_file, IFileSystem a_fileSystem)
        {
            Stream stream;
            if (a_fileSystem.Load(a_file, out stream))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(stream);

                stream.Dispose();

                for (XmlNode node = doc.FirstChild; node != null; node = node.NextSibling)
                {
                    if (node.Name == "Localization")
                    {
                        AddLocalization(node);

                        break;
                    }
                }
            }
        }
        public static void LoadFontTable(string a_file, IFileSystem a_fileSystem)
        {
            Stream stream;
            if (a_fileSystem.Load(a_file, out stream))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(stream);

                stream.Dispose();

                for (XmlNode node = doc.FirstChild; node != null; node = node.NextSibling)
                {
                    if (node.Name == "FontTable")
                    {
                        AddFontTable(node);

                        break;
                    }
                }
            }
        }

        public static void AddTranslationTag(string a_tag, string a_string)
        {
            if (TranslationTable == null)
            {
                TranslationTable = new Dictionary<string, string>();
            }

            if (TranslationTable.ContainsKey(a_tag))
            {
                TranslationTable[a_tag] = a_string;

                return;
            }

            TranslationTable.Add(a_tag, a_string);
        }
        public static void AddFontTag(string a_tag, string a_name)
        {
            if (FontTable == null)
            {
                FontTable = new Dictionary<string, string>();
            }

            if (FontTable.ContainsKey(a_tag))
            {
                FontTable[a_tag] = a_name;

                return;
            }

            FontTable.Add(a_tag, a_name);
        }

        public static string Translate(this string a_tag)
        {
            if (TranslationTable == null)
            {
                return string.Empty;
            }

            if (TranslationTable.ContainsKey(a_tag))
            {
                return TranslationTable[a_tag];
            }

            return string.Empty;
        }
        
        public static string GetFontName(string a_fontTag)
        {
            if (FontTable == null)
            {
                return string.Empty;
            }

            if (FontTable.ContainsKey(a_fontTag))
            {
                return FontTable[a_fontTag];
            }

            return string.Empty;
        }
    }
}