using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    /// <summary>
    /// Represents an operator that measures temperature with Pt100 and Pt1000 sensors
    /// from a Industrial PTC Bricklet.
    /// </summary>
    [DefaultProperty(nameof(Uid))]
    [Description("Measures temperature with Pt100 and Pt1000 sensors from a Industrial PTC Bricklet.")]
    public class IndustrialPTC : Combinator<IPConnection, int>
    {
        /// <summary>
        /// Gets or sets the bricklet device UID.
        /// </summary>
        [TypeConverter(typeof(UidConverter))]
        [DeviceType(typeof(BrickletIndustrialPTC))]
        [Description("The bricklet device UID.")]
        public string Uid { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the period between sample event callbacks.
        /// A value of zero disables event reporting.
        /// </summary>
        [Description("Specifies the period between sample event callbacks. A value of zero disables event reporting.")]
        public long Period { get; set; } = 1000;

        /// <summary>
        /// Gets or sets a value specifying the moving average window length for temperature.
        /// A value of 1 turns off averaging.
        /// </summary>
        [Range(1, 1000)]
        [Description("Specifies the moving average window length for temperature. A value of 1 turns off averaging.")]
        public int MovingAverageLengthTemperature { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value specifying the moving average window length for resistance.
        /// A value of 1 turns off averaging.
        /// </summary>
        [Range(1, 1000)]
        [Description("Specifies the moving average window length for resistance. A value of 1 turns off averaging.")]
        public int MovingAverageLengthResistance { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value specifying the wire mode of the sensor.
        /// </summary>
        [Description("Specifies the wire mode of the sensor.")]
        public IndustrialPTCWireMode WireMode { get; set; } = IndustrialPTCWireMode.WireMode2;

        /// <summary>
        /// Gets or sets a value specifying the behavior of the status LED.
        /// </summary>
        [Description("Specifies the behavior of the status LED.")]
        public IndustrialPTCStatusLedConfig StatusLed { get; set; } = IndustrialPTCStatusLedConfig.ShowStatus;

        /// <inheritdoc/>
        public override string ToString()
        {
            return BrickletIndustrialPTC.DEVICE_DISPLAY_NAME;
        }

        /// <summary>
        /// Measures temperature from a Industrial PTC Bricklet
        /// </summary>
        /// <param name="source">
        /// A sequence containing the TCP/IP connection to the Brick Daemon.
        /// </param>
        /// <returns>the 
        /// A sequence of <see cref="int"/> values representing the
        /// temperature measurements from the Industrial PTC Bricklet
        /// in 1/100ths of a degree celsius.
        /// </returns>
        public override IObservable<int> Process(IObservable<IPConnection> source)
        {
            return source.SelectStream(connection =>
            {
                var device = new BrickletIndustrialPTC(Uid, connection);
                connection.Connected += (sender, e) =>
                {
                    device.SetStatusLEDConfig((byte)StatusLed);
                    device.SetWireMode((byte)WireMode);
                    device.SetMovingAverageConfiguration(MovingAverageLengthResistance, MovingAverageLengthTemperature);
                    device.SetTemperatureCallbackConfiguration(Period, false, 'x', 0, 0);
                };

                return Observable.Create<int>(observer =>
                {
                    BrickletIndustrialPTC.TemperatureEventHandler handler = (sender, temperature) =>
                    {
                        observer.OnNext(temperature);
                    };

                    device.TemperatureCallback += handler;
                    return Disposable.Create(() =>
                    {
                        try { device.SetTemperatureCallbackConfiguration(0, false, 'x', 0, 0); }
                        catch (NotConnectedException) { }
                        device.TemperatureCallback -= handler;
                    });
                });
            });
        }
    }

    /// <summary>
    /// Specifies the wire mode configuration of a Industrial PTC Bricklet
    /// corresponding the wires of the sensor.
    /// </summary>
    public enum IndustrialPTCWireMode : byte
    {
        /// <summary>
        /// The bricklet will use a 2-wire sensor.
        /// </summary>
        WireMode2 = BrickletIndustrialPTC.WIRE_MODE_2,

        /// <summary>
        /// The bricklet will use a 2-wire sensor.
        /// </summary>
        WireMode3 = BrickletIndustrialPTC.WIRE_MODE_3,

        /// <summary>
        /// The bricklet will use a 4-wire sensor.
        /// </summary>
        WireMode4 = BrickletIndustrialPTC.WIRE_MODE_4,
    }

    /// <summary>
    /// Specifies the behavior of the Industrial PTC Bricklet status LED.
    /// </summary>
    public enum IndustrialPTCStatusLedConfig : byte
    {
        /// <summary>
        /// The status LED will be permanently OFF.
        /// </summary>
        Off = BrickletIndustrialPTC.STATUS_LED_CONFIG_OFF,

        /// <summary>
        /// The status LED will be permanently ON as long as the bricklet is powered.
        /// </summary>
        On = BrickletIndustrialPTC.STATUS_LED_CONFIG_ON,

        /// <summary>
        /// The status LED will change state periodically every second.
        /// </summary>
        ShowHeartbeat = BrickletIndustrialPTC.STATUS_LED_CONFIG_SHOW_HEARTBEAT,

        /// <summary>
        /// The LED will show communication traffic between Brick and Bricklet,
        /// flickering once for every 10 received data packets.
        /// </summary>
        ShowStatus = BrickletIndustrialPTC.STATUS_LED_CONFIG_SHOW_STATUS
    }
}
