using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Bonsai.Tinkerforge
{
    public class TinkerforgeHelpers
    {
        /// <summary>
        /// Tinkerforge modules have a standard int->Name lookup
        /// </summary>
        public static class TinkerForgeDeviceLookup
        {
            public static Dictionary<int, string> Defaults = new Dictionary<int, string>
            {
                { 13,  "Master Brick"},
                { 2131, "Ambient Light Bricklet 3.0"},
                { 2147, "CO2 Bricklet 2.0" }
            };
        }

        /// <summary>
        /// Data representation of a connected TinkerForge module
        /// </summary>
        public class DeviceData
        {
            public string UID; // Unique module ID
            public string ConnectedUID; // IDs of connected modules
            public char Position; // Position in the network
            public int DeviceIdentifier; // Number corresponding to device name

            public DeviceData()
            {

            }

            public DeviceData(string uid, string connectedUid, char position, int deviceIdentifier)
            {
                UID = uid;
                ConnectedUID = connectedUid;
                Position = position;
                DeviceIdentifier = deviceIdentifier;
            }

            public override string ToString()
            {
                return $"{TinkerForgeDeviceLookup.Defaults[DeviceIdentifier]}:{UID}:{Position}:{ConnectedUID}";
            }
        }
    }
}
