using AlgernonCommons.XML;
using ColossalFramework.IO;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine;
using static FPSCamera.Utils.MathUtils;

namespace FPSCamera.Settings
{
    [XmlRoot("FPSCameraOffsets")]
    public sealed class OffsetsSettings : SettingsXMLBase, IXmlSerializable
    {
        /// <summary>
        /// Settings file name
        /// </summary>
        [XmlIgnore]
        private static readonly string SettingsFileName = Path.Combine(DataLocation.localApplicationData, "FPSCamera_Continued_Offsets.xml");

        internal static void Load()
        {
            if (File.Exists(SettingsFileName))
                XMLFileUtils.Load<OffsetsSettings>(SettingsFileName);
            else
                Save();
        }

        internal static void Save() => XMLFileUtils.Save<OffsetsSettings>(SettingsFileName);

        [XmlElement("Offset")]
        public Dictionary<string, Positioning> XMLOffsets { get => Offsets; set => Offsets = value; }
        [XmlIgnore]
        internal static Dictionary<string, Positioning> Offsets = new Dictionary<string, Positioning>
        {
            ["Bus"] = new Positioning(new Vector3(0f, .2f, 2.1f)),
            ["Biofuel Bus 01"] = new Positioning(new Vector3(0f, .2f, 2.1f)),
            ["School Bus 01"] = new Positioning(new Vector3(0f, .3f, .75f)),
            ["Tram"] = new Positioning(new Vector3(0f, .23f, 1.04f)),
            ["Tram Middle"] = new Positioning(new Vector3(1.82f, -1.32f, -6.24f), Quaternion.Euler(-3.6f, 0.6f, 0f)),
            ["Tram End"] = new Positioning(new Vector3(0f, -2.76f, -5f), Quaternion.Euler(0f, 180f, 0f)),
            ["Metro"] = new Positioning(new Vector3(0f, .76f, 4.35f)),
            ["Metro Passenger"] = new Positioning(new Vector3(2.43f, -1.05f, .11f), Quaternion.Euler(-8f, -6.3f, 0f)),
            ["Monorail Passenger"] = new Positioning(new Vector3(1.2f, -1.7f, -.32f)),
            ["Train Engine"] = new Positioning(new Vector3(0f, .9f, 3.84f)),
            ["Train Passenger"] = new Positioning(new Vector3(1.78f, -.45f, -2.44f), Quaternion.Euler(-7.35f, -4.5f, 0f)),
            ["Train Cargo Engine"] = new Positioning(new Vector3(0f, .9f, 3.19f)),
            ["Train Cargo"] = new Positioning(new Vector3(0f, -.59f, 0f)),
            ["Bicycle02"] = new Positioning(new Vector3(0f, -.4f, -3f)),
            ["Personal Electric Transport Ride"] = new Positioning(new Vector3(0f, -.4f, -2f)),
            ["Camper Trailer 02"] = new Positioning(new Vector3(0f, -2.2f, 0f)),
            ["Tractor"] = new Positioning(new Vector3(0f, .7f, -.46f)),
            ["Forest Forwarder 01"] = new Positioning(new Vector3(0f, .96f, 1.16f)),
            ["Farm Truck 01"] = new Positioning(new Vector3(0f, .5f, -1f)),
        };

        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            if (reader.IsEmptyElement) return;

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "Offset")
                {
                    string name = reader.GetAttribute("Name");
                    var offset = new Positioning(Vector3.zero);

                    while (reader.Read())
                    {

                        // Positioning
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            // Position
                            if (reader.Name == "Position")
                            {
                                reader.ReadStartElement("Position");
                                offset.pos.x = reader.ReadElementContentAsFloat("x", "");
                                offset.pos.y = reader.ReadElementContentAsFloat("y", "");
                                offset.pos.z = reader.ReadElementContentAsFloat("z", "");
                                reader.ReadEndElement();
                            }
                            // Rotation
                            if (reader.Name == "Rotation")
                            {
                                reader.ReadStartElement("Rotation");
                                offset.rotation.x = reader.ReadElementContentAsFloat("x", "");
                                offset.rotation.y = reader.ReadElementContentAsFloat("y", "");
                                offset.rotation.z = reader.ReadElementContentAsFloat("z", "");
                                offset.rotation.w = reader.ReadElementContentAsFloat("w", "");
                                reader.ReadEndElement();
                            }
                        }
                        else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Offset")
                        {
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(name))
                    {
                        XMLOffsets[name] = offset;
                    }
                }
            }
        }


        public void WriteXml(XmlWriter writer)
        {
            foreach (var kvp in XMLOffsets)
            {
                writer.WriteStartElement("Offset");
                writer.WriteAttributeString("Name", kvp.Key);

                writer.WriteStartElement("Positioning");

                // Position
                writer.WriteStartElement("Position");
                writer.WriteElementString("x", kvp.Value.pos.x.ToString("F2"));
                writer.WriteElementString("y", kvp.Value.pos.y.ToString("F2"));
                writer.WriteElementString("z", kvp.Value.pos.z.ToString("F2"));
                writer.WriteEndElement(); // Position

                // Rotation
                writer.WriteStartElement("Rotation");
                writer.WriteElementString("x", kvp.Value.rotation.x.ToString("F2"));
                writer.WriteElementString("y", kvp.Value.rotation.y.ToString("F2"));
                writer.WriteElementString("z", kvp.Value.rotation.z.ToString("F2"));
                writer.WriteElementString("w", kvp.Value.rotation.w.ToString("F2"));
                writer.WriteEndElement(); // Rotation

                writer.WriteEndElement(); // Positioning

                writer.WriteEndElement(); // Offset
            }
        }

        public XmlSchema GetSchema() => null;

        //Ignore not used settings 
        [XmlIgnore]
        public new string Language { get; set; }

        [XmlIgnore]
        public new string WhatsNewVersion { get; set; }

        [XmlIgnore]
        public new bool XMLDetailedLogging { get; set; }
    }
}
