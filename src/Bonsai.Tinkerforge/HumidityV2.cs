using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    /// <summary>
    /// Represents an operator that measures relative humidity from a Humidity Bricklet 2.0.
    /// </summary>
    [DefaultProperty(nameof(Uid))]
    [Description("Measures relative humidity from a Humidity Bricklet 2.0.")]
    public class HumidityV2 : Combinator<IPConnection, int>
    {
        /// <summary>
        /// Gets or sets the bricklet device UID.
        /// </summary>
        [TypeConverter(typeof(UidConverter))]
        [DeviceType(typeof(BrickletHumidityV2))]
        [Description("The bricklet device UID.")]
        public string Uid { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the period between sample event callbacks.
        /// A value of zero disables event reporting.
        /// </summary>
        [Description("Specifies the period between sample event callbacks. A value of zero disables event reporting.")]
        public long Period { get; set; } = 1000;

        /// <summary>
        /// Gets or sets a value specifying the heater configuration. The heater can be
        /// used to dy the sensor in extremely wet conditions.
        /// </summary>
        [Description("Specifies the heater configuration. The heater can be used to dry the sensor in extremely wet conditions.")]
        public HumidityV2HeaterConfig Heater { get; set; } = HumidityV2HeaterConfig.Disabled;

        /// <summary>
        /// Gets or sets a value specifying the moving average window length (number of samples) for temperature.
        /// A value of 1 turns off averaging.
        /// </summary>
        [Range(1, 1000)]
        [Description("Specifies the moving average window length for temperature. A value of 1 turns off averaging.")]
        public int MovingAverageLengthTemperature { get; set; } = 5;

        /// <summary>
        /// Gets or sets a value specifying the moving average window length (number of samples) for humidity.
        /// A value of 1 turns off averaging.
        /// </summary>
        [Range(1, 1000)]
        [Description("Specifies the moving average window length for humidity. A value of 1 turns off averaging.")]
        public int MovingAverageLengthHumidity { get; set; } = 5;

        /// <summary>
        /// Gets or sets a value specifying the behavior of the status LED.
        /// </summary>
        [Description("Specifies the behavior of the status LED.")]
        public HumidityV2StatusLedConfig StatusLed { get; set; } = HumidityV2StatusLedConfig.ShowStatus;

        /// <inheritdoc/>
        public override string ToString()
        {
            return BrickletHumidityV2.DEVICE_DISPLAY_NAME;
        }

        /// <summary>
        /// Measures relative humidity from a Humidity Bricklet 2.0.
        /// </summary>
        /// <param name="source">
        /// A sequence containing the TCP/IP connection to the Brick Daemon.
        /// </param>
        /// <returns>the 
        /// A sequence of <see cref="int"/> values representing the
        /// humidity measurements from the Humidity Bricklet 2.0. in
        /// hundredths of percentage points (%RH)
        /// </returns>
        public override IObservable<int> Process(IObservable<IPConnection> source)
        {
            if (string.IsNullOrEmpty(Uid))
            {
                throw new ArgumentException("A device Uid must be specified", "Uid");
            }

            return source.SelectStream(connection =>
            {
                var device = new BrickletHumidityV2(Uid, connection);
                connection.Connected += (sender, e) =>
                {
                    device.SetStatusLEDConfig((byte)StatusLed);
                    device.SetHeaterConfiguration((byte)Heater);
                    device.SetMovingAverageConfiguration(MovingAverageLengthHumidity, MovingAverageLengthTemperature);
                    device.SetHumidityCallbackConfiguration(Period, false, 'x', 0, 0);
                };

                return Observable.Create<int>(observer =>
                {
                    BrickletHumidityV2.HumidityEventHandler handler = (sender, humidity) =>
                    {
                        observer.OnNext(humidity);
                    };

                    device.HumidityCallback += handler;
                    return Disposable.Create(() =>
                    {
                        try { device.SetHumidityCallbackConfiguration(0, false, 'x', 0, 0); }
                        catch (NotConnectedException) { }
                        device.HumidityCallback -= handler;
                    });
                });
            });
        }
    }

    /// <summary>
    /// Specifies the configuration of the heater.
    /// </summary>
    public enum HumidityV2HeaterConfig : byte
    {
        /// <summary>
        /// Specifies that the heater will be disabled.
        /// </summary>
        Disabled = BrickletHumidityV2.HEATER_CONFIG_DISABLED,

        /// <summary>
        /// Specifies that the heater will be enabled.
        /// </summary>
        Enabled = BrickletHumidityV2.HEATER_CONFIG_ENABLED
    }

    /// <summary>
    /// Specifies the behavior of the Humidity Bricklet 2.0. status LED.
    /// </summary>
    public enum HumidityV2StatusLedConfig : byte
    {
        /// <summary>
        /// The status LED will be permanently OFF.
        /// </summary>
        Off = BrickletHumidityV2.STATUS_LED_CONFIG_OFF,

        /// <summary>
        /// The status LED will be permanently ON as long as the bricklet is powered.
        /// </summary>
        On = BrickletHumidityV2.STATUS_LED_CONFIG_ON,

        /// <summary>
        /// The status LED will change state periodically every second.
        /// </summary>
        ShowHeartbeat = BrickletHumidityV2.STATUS_LED_CONFIG_SHOW_HEARTBEAT,

        /// <summary>
        /// The LED will show communication traffic between Brick and Bricklet,
        /// flickering once for every 10 received data packets.
        /// </summary>
        ShowStatus = BrickletHumidityV2.STATUS_LED_CONFIG_SHOW_STATUS
    }
}
