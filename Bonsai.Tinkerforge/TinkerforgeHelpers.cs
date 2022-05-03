﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Tinkerforge
{
    public class TinkerforgeHelpers
    {
        public static class TinkerForgeNameLookup
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
            public string DeviceIdentifier; // Number corresponding to device name

            public DeviceData(string uid, string connectedUid, char position, int deviceIdentifier)
            {
                UID = uid;
                ConnectedUID = connectedUid;
                Position = position;
                DeviceIdentifier = TinkerforgeHelpers.TinkerForgeNameLookup.Defaults[deviceIdentifier];
            }
        }
    }
}