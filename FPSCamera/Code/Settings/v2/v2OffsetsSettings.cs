using AlgernonCommons.XML;
using ColossalFramework.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine;
using static FPSCamera.Utils.MathUtils;

namespace FPSCamera.Settings.v2
{
    [XmlRoot("CamOffset")]
    public class v2OffsetsSettings : SettingsXMLBase, IXmlSerializable
    {
        [XmlIgnore]
        private static readonly string SettingsFileName = Path.Combine(DataLocation.executableDirectory, "FPSCameraOffset.xml");

        internal static void Load()
        {
            offsets.Clear();
            using (var reader = new StreamReader(SettingsFileName))
            {
                var xmlSerializer = new XmlSerializer(typeof(v2OffsetsSettings));
                if (!(xmlSerializer.Deserialize(reader) is v2OffsetsSettings xmlFile))
                {
                    throw new FileLoadException("couldn't deserialize XML file ", SettingsFileName);
                }
                foreach (var kvp in offsets)
                {
                    OffsetsSettings.Offsets[kvp.Key] = kvp.Value;
                }
            }
        }
        private static Dictionary<string, Positioning> offsets { get; set; } = new Dictionary<string, Positioning>();

        public XmlSchema GetSchema() => null;

        public void ReadXml(XmlReader reader)
        {
            reader.ReadToDescendant("_offsets");

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    string tag = reader.Name;
                    string value = reader.ReadElementContentAsString();

                    string convertedTag = TagToStr(tag);

                    var splitValues = value.Split(',');
                    float x = float.Parse(splitValues[0]);
                    float y = float.Parse(splitValues[1]);
                    float z = float.Parse(splitValues[2]);
                    float eulerX = float.Parse(splitValues[3]);
                    float eulerY = float.Parse(splitValues[4]);

                    offsets[convertedTag] = new Positioning(new Vector3(z, y, x), Quaternion.Euler(eulerY, eulerX, 0f));
                }
            }
        }

        private static string TagToStr(string tag)
        {
            string str = "";
            for (int i = 1; i < tag.Length; i++)
            {
                if (tag[i] == '_')
                {
                    if (i + 1 < tag.Length && tag[i + 1] == '_')
                    {
                        str += '_';
                        i++;
                    }
                    else if (i + 3 < tag.Length && byte.TryParse(tag.Substring(i + 1, 3), out var ch))
                    {
                        str += (char)ch;
                        i += 3;
                    }
                    else
                    {
                        throw new Exception($"Config import: xml tag({tag}) is invalid");
                    }
                }
                else if (char.IsLetterOrDigit(tag[i]))
                {
                    str += tag[i];
                }
                else
                {
                    throw new Exception($"Config import: xml tag({tag}) contains invalid character '{tag[i]}'");
                }
            }
            return str;
        }

        public void WriteXml(XmlWriter writer) { }
    }
}
