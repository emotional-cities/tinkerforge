using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    /// <summary>
    /// Represents an operator that measures temperature using thermocouple wires
    /// from a Thermocouple Bricklet 2.0.
    /// </summary>
    [Combinator]
    [DefaultProperty(nameof(Uid))]
    [Description("Measures temperature using thermocouple wires from a Thermocouple Bricklet 2.0.")]
    public class ThermocoupleV2
    {
        /// <summary>
        /// Gets or sets the bricklet device UID.
        /// </summary>
        [TypeConverter(typeof(UidConverter))]
        [DeviceType(typeof(BrickletThermocoupleV2))]
        [Description("The bricklet device UID.")]
        public string Uid { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the period between sample event callbacks.
        /// A value of zero disables event reporting.
        /// </summary>
        [Description("Specifies the period between sample event callbacks. A value of zero disables event reporting.")]
        public long Period { get; set; } = 1000;

        /// <summary>
        /// Gets or sets a value specifying the averaging size. 
        /// This setting will affect conversion time.
        /// </summary>
        [Description("Specifies the averaging size. This setting will affect conversion time.")]
        public ThermocoupleV2AveragingSize Averaging { get; set; } = ThermocoupleV2AveragingSize.Averaging16;

        /// <summary>
        /// Gets or sets a value specifying the type of the thermocouple.
        /// Different thermocouple types use different types of metals for their wires.
        /// </summary>
        [Description("Specifies the type of the thermocouple. Different thermocouple types use different types of metals for their wires.")]
        public ThermocoupleV2ThermocoupleType Type { get; set; } = ThermocoupleV2ThermocoupleType.TypeK;

        /// <summary>
        /// Gets or sets a value specifying the filter frequency.
        /// Should be configures according to utility frequency.
        /// </summary>
        [Description("Specifies the filter frequency. Should be configured according to utility frequency.")]
        public ThermocoupleV2FilterOption Filter { get; set; } = ThermocoupleV2FilterOption.Filter50Hz;

        /// <summary>
        /// Gets or sets a value specifying the behavior of the status LED.
        /// </summary>
        [Description("Specifies the behavior of the status LED.")]
        public ThermocoupleV2StatusLedConfig StatusLed { get; set; } = ThermocoupleV2StatusLedConfig.ShowStatus;

        /// <inheritdoc/>
        public override string ToString()
        {
            return BrickletThermocoupleV2.DEVICE_DISPLAY_NAME;
        }

        /// <summary>
        /// Measures temperature using thermocouple wires from a Thermocouple Bricklet 2.0.
        /// </summary>
        /// <param name="source">
        /// A sequence containing the TCP/IP connection to the Brick Daemon.
        /// </param>
        /// <returns>the 
        /// A sequence of <see cref="int"/> values representing the
        /// measurements from the Thermocouple Bricklet 2.0.
        /// in units of 1/100th of a degree (celsius).
        /// </returns>
        public IObservable<int> Process(IObservable<IPConnection> source)
        {
            return source.SelectStream(connection =>
            {
                var device = new BrickletThermocoupleV2(Uid, connection);
                connection.Connected += (sender, e) =>
                {
                    device.SetStatusLEDConfig((byte)StatusLed);
                    device.SetConfiguration((byte)Averaging, (byte)Type, (byte)Filter);
                    device.SetTemperatureCallbackConfiguration(Period, false, 'x', 0, 1);
                };

                return Observable.FromEventPattern<BrickletThermocoupleV2.TemperatureEventHandler, int>(
                    handler => device.TemperatureCallback += handler,
                    handler => device.TemperatureCallback -= handler)
                    .Select(evt => evt.EventArgs)
                    .Finally(() =>
                    {
                        try { device.SetTemperatureCallbackConfiguration(0, false, 'x', 0, 1); }
                        catch (NotConnectedException) { } // best effort
                });
            });
        }
    }

    /// <summary>
    /// Specifies the averaging size of the Thermocouple Bricklet 2.0.
    /// </summary>
    public enum ThermocoupleV2AveragingSize : byte
    {
        /// <summary>
        /// Specifies an averaging size of 1 sample.
        /// </summary>
        Averaging1 = BrickletThermocoupleV2.AVERAGING_1,

        /// <summary>
        /// Specifies an averaging size of 2 samples.
        /// </summary>
        Averaging2 = BrickletThermocoupleV2.AVERAGING_2,

        /// <summary>
        /// Specifies an averaging size of 4 samples.
        /// </summary>
        Averaging4 = BrickletThermocoupleV2.AVERAGING_4,

        /// <summary>
        /// Specifies an averaging size of 8 samples.
        /// </summary>
        Averaging8 = BrickletThermocoupleV2.AVERAGING_8,

        /// <summary>
        /// Specifies an averaging size of 16 samples.
        /// </summary>
        Averaging16 = BrickletThermocoupleV2.AVERAGING_16
    }

    /// <summary>
    /// Specifies the thermocouple type used with the Thermocouple Bricklet 2.0.
    /// </summary>
    public enum ThermocoupleV2ThermocoupleType : byte
    {
        /// <summary>
        /// Specifies a thermocouple of type B will be used.
        /// </summary>
        TypeB = BrickletThermocoupleV2.TYPE_B,

        /// <summary>
        /// Specifies a thermocouple of type E will be used.
        /// </summary>
        TypeE = BrickletThermocoupleV2.TYPE_E,

        /// <summary>
        /// Specifies a thermocouple of type J will be used.
        /// </summary>
        TypeJ = BrickletThermocoupleV2.TYPE_J,

        /// <summary>
        /// Specifies a thermocouple of type K will be used.
        /// </summary>
        TypeK = BrickletThermocoupleV2.TYPE_K,

        /// <summary>
        /// Specifies a thermocouple of type N will be used.
        /// </summary>
        TypeN = BrickletThermocoupleV2.TYPE_N,

        /// <summary>
        /// Specifies a thermocouple of type R will be used.
        /// </summary>
        TypeR = BrickletThermocoupleV2.TYPE_R,

        /// <summary>
        /// Specifies a thermocouple of type S will be used.
        /// </summary>
        TypeS = BrickletThermocoupleV2.TYPE_S,

        /// <summary>
        /// Specifies a thermocouple of type T will be used.
        /// </summary>
        TypeT = BrickletThermocoupleV2.TYPE_T,

        /// <summary>
        /// Specifies a thermocouple of type G8 will be used.
        /// If this thermocouple type is used the returned temperature value
        /// will be calculated by the formula 8 * 1.6 * 2^17 * Vin.
        /// </summary>
        TypeG8 = BrickletThermocoupleV2.TYPE_G8,

        /// <summary>
        /// Specifies a thermocouple of type G32 will be used.
        /// If this thermocouple type is used the returned temperature value
        /// will be calculated by the formula 32 * 1.6 * 2^17 * Vin.
        /// </summary>
        TypeG32 = BrickletThermocoupleV2.TYPE_G32
    }

    /// <summary>
    /// Specifies the filtering option for the Thermocouple Bricklet 2.0.
    /// </summary>
    public enum ThermocoupleV2FilterOption : byte
    {
        /// <summary>
        /// Specifies a 50Hz filter will be used giving a conversion time of 98 + (samples - 1) * 20.
        /// </summary>
        Filter50Hz = BrickletThermocoupleV2.FILTER_OPTION_50HZ,

        /// <summary>
        /// Specifies a 60Hz filter will be used giving a conversion time of 82 + (samples - 1) * 16.67.
        /// </summary>
        Filter60Hz = BrickletThermocoupleV2.FILTER_OPTION_60HZ
    }

    /// <summary>
    /// Specifies the behavior of the Thermocouple Bricklet 2.0.
    /// </summary>
    public enum ThermocoupleV2StatusLedConfig : byte
    {
        /// <summary>
        /// The status LED will be permanently OFF.
        /// </summary>
        Off = BrickletThermocoupleV2.STATUS_LED_CONFIG_OFF,

        /// <summary>
        /// The status LED will be permanently ON as long as the bricklet is powered.
        /// </summary>
        On = BrickletThermocoupleV2.STATUS_LED_CONFIG_ON,

        /// <summary>
        /// The status LED will change state periodically every second.
        /// </summary>
        ShowHeartbeat = BrickletThermocoupleV2.STATUS_LED_CONFIG_SHOW_HEARTBEAT,

        /// <summary>
        /// The LED will show communication traffic between Brick and Bricklet,
        /// flickering once for every 10 received data packets.
        /// </summary>
        ShowStatus = BrickletThermocoupleV2.STATUS_LED_CONFIG_SHOW_STATUS
    }
}
