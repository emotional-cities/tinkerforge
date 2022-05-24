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
            Console.WriteLine(DeviceIdentifier);
            return $"{TinkerforgeDeviceLookup.Defaults[DeviceIdentifier]}:{Uid}";
        }
    }

    /// <summary>
    /// Tinkerforge modules have a standard int->Name lookup
    /// </summary>
    internal class TinkerforgeDeviceLookup
    {
        public static readonly Dictionary<int, string> Defaults = new Dictionary<int, string>
        {
            { global::Tinkerforge.BrickMaster.DEVICE_IDENTIFIER,  global::Tinkerforge.BrickMaster.DEVICE_DISPLAY_NAME},
            { global::Tinkerforge.BrickletAmbientLightV3.DEVICE_IDENTIFIER,  global::Tinkerforge.BrickletAmbientLightV3.DEVICE_DISPLAY_NAME},
            { global::Tinkerforge.BrickletCO2V2.DEVICE_IDENTIFIER, global::Tinkerforge.BrickletCO2V2.DEVICE_DISPLAY_NAME },
            { global::Tinkerforge.BrickletAccelerometer.DEVICE_IDENTIFIER, global::Tinkerforge.BrickletAccelerometer.DEVICE_DISPLAY_NAME },
            { global::Tinkerforge.BrickletAirQuality.DEVICE_IDENTIFIER, global::Tinkerforge.BrickletAirQuality.DEVICE_DISPLAY_NAME },
            { global::Tinkerforge.BrickletGPSV2.DEVICE_IDENTIFIER, global::Tinkerforge.BrickletGPSV2.DEVICE_DISPLAY_NAME },
            { global::Tinkerforge.BrickletHumidityV2.DEVICE_IDENTIFIER, global::Tinkerforge.BrickletHumidityV2.DEVICE_DISPLAY_NAME },
            { global::Tinkerforge.BrickletIndustrialAnalogOutV2.DEVICE_IDENTIFIER, global::Tinkerforge.BrickletIndustrialAnalogOutV2.DEVICE_DISPLAY_NAME },
            { global::Tinkerforge.BrickletIndustrialPTC.DEVICE_IDENTIFIER, global::Tinkerforge.BrickletIndustrialPTC.DEVICE_DISPLAY_NAME },
            { global::Tinkerforge.BrickletParticulateMatter.DEVICE_IDENTIFIER, global::Tinkerforge.BrickletParticulateMatter.DEVICE_DISPLAY_NAME },
            { global::Tinkerforge.BrickletSoundPressureLevel.DEVICE_IDENTIFIER, global::Tinkerforge.BrickletSoundPressureLevel.DEVICE_DISPLAY_NAME },
            { global::Tinkerforge.BrickletThermocoupleV2.DEVICE_IDENTIFIER, global::Tinkerforge.BrickletThermocoupleV2.DEVICE_DISPLAY_NAME },
            { global::Tinkerforge.BrickletAnalogInV3.DEVICE_IDENTIFIER, global::Tinkerforge.BrickletAnalogInV3.DEVICE_DISPLAY_NAME }
        };
    }
}
