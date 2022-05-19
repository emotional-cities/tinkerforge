using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Tinkerforge
{
    /// <summary>
    /// Data representation of a connected TinkerForge module
    /// </summary>
    public class DeviceData
    {
        public string Uid; // Unique module ID
        public string ConnectedUid; // IDs of connected modules
        public char Position; // Position in the network
        public int DeviceIdentifier; // Number corresponding to device name

        // Parameterless constructor required for serialization
        public DeviceData()
        {
        }

        public DeviceData(string uid, string connectedUid, char position, int deviceIdentifier)
        {
            Uid = uid;
            ConnectedUid = connectedUid;
            Position = position;
            DeviceIdentifier = deviceIdentifier;
        }

        public override string ToString()
        {
            return $"{TinkerforgeDeviceLookup.Defaults[DeviceIdentifier]}:{Uid}:{Position}:{ConnectedUid}";
        }
    }

    /// <summary>
    /// Tinkerforge modules have a standard int->Name lookup
    /// </summary>
    internal class TinkerforgeDeviceLookup
    {
        public static readonly Dictionary<int, string> Defaults = new Dictionary<int, string>
        {
            { 13,  "Master Brick"},
            { 2131, "Ambient Light Bricklet 3.0"},
            { 2147, "CO2 Bricklet 2.0" }
        };
    }
}
