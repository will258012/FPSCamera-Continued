using AlgernonCommons.XML;
using ColossalFramework.IO;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using static FPSCamera.Utils.MathUtils;

namespace FPSCamera.Settings
{
    [XmlRoot("FPSCameraOffsets")]
    public sealed class OffsetsSettings : SettingsXMLBase
    {
        /// <summary>
        /// Settings file name
        /// </summary>
        [XmlIgnore]
        private static readonly string SettingsFileName = Path.Combine(DataLocation.localApplicationData, "FPSCamera_Continued_Offsets.xml");

        internal static void Load() => XMLFileUtils.Load<OffsetsSettings>(SettingsFileName);

        internal static void Save() => XMLFileUtils.Save<OffsetsSettings>(SettingsFileName);

        [XmlElement("Offset")]
        public List<OffsetEntry> XMLOffsets { get => Offsets.Select(kvp => new OffsetEntry { Key = kvp.Key, Value = kvp.Value }).ToList(); set => Offsets = value.ToDictionary(entry => entry.Key, entry => entry.Value); }
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
        /// <summary>
        /// Make <see cref="Dictionary{TKey, TValue}"/> can be serialized
        /// </summary>
        public class OffsetEntry
        {
            [XmlAttribute("Name")]
            public string Key { get; set; }
            public Positioning Value { get; set; }
        }

        //Ignore not used settings 
        [XmlIgnore]
        public new string Language { get; set; }

        [XmlIgnore]
        public new string WhatsNewVersion { get; set; }

        [XmlIgnore]
        public new bool XMLDetailedLogging { get; set; }
    }
}
