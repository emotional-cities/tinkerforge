using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    /// <summary>
    /// Represents an operator that measures ambient illuminance from an Ambient Light Bricklet 3.0.
    /// </summary>
    [DefaultProperty(nameof(Uid))]
    [Description("Measures ambient illuminance from an Ambient Light Bricklet 3.0.")]
    public class AmbientLightV3 : Combinator<IPConnection, long>
    {
        /// <summary>
        /// Gets or sets the bricklet device UID.
        /// </summary>
        [TypeConverter(typeof(UidConverter))]
        [DeviceType(typeof(BrickletAmbientLightV3))]
        [Description("The bricklet device UID.")]
        public string Uid { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the period between sample event callbacks.
        /// A value of zero disables event reporting.
        /// </summary>
        [Description("Specifies the period between sample event callbacks. A value of zero disables event reporting.")]
        public long Period { get; set; } = 1000;

        /// <summary>
        /// Gets or sets a value specifying the measurement range for the ambient light sensor.
        /// Smaller illumination ranges increase the resolution of the data.
        /// </summary>
        [Description("Specifies the measurement range for the ambient light sensor. Smaller illumination ranges increase the resolution of the data.")]
        public AmbientLightV3IlluminanceRange IlluminanceRange { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the integration time of the sensor. 
        /// Larger integration times result in less noise in the data.
        /// </summary>
        [Description("Specifies the integration time of the sensor. Larger integration times result in less noise in the data.")]
        public AmbientLightV3IntegrationTime IntegrationTime { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the behavior of the status LED.
        /// </summary>
        [Description("Specifies the behavior of the status LED.")]
        public AmbientLightV3StatusLedConfig StatusLed { get; set; } = AmbientLightV3StatusLedConfig.ShowStatus;

        /// <inheritdoc/>
        public override string ToString()
        {
            return BrickletAmbientLightV3.DEVICE_DISPLAY_NAME;
        }

        /// <summary>
        /// Measures ambient light level from an Ambient Light Bricklet 3.0.
        /// </summary>
        /// <param name="source">
        /// A sequence containing the TCP/IP connection to the Brick Daemon.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="long"/> values representing the
        /// illuminance measurements from the Ambient Light Bricklet 3.0. in units of 1/100lux.
        /// </returns>
        public override IObservable<long> Process(IObservable<IPConnection> source)
        {
            return source.SelectStream(connection =>
            {
                var device = new BrickletAmbientLightV3(Uid, connection);
                connection.Connected += (sender, e) =>
                {
                    device.SetStatusLEDConfig((byte)StatusLed);
                    device.SetConfiguration((byte)IlluminanceRange, (byte)IntegrationTime);
                    device.SetIlluminanceCallbackConfiguration(Period, false, 'x', 0, 1);
                };

                return Observable.FromEventPattern<BrickletAmbientLightV3.IlluminanceEventHandler, long>(
                    handler => device.IlluminanceCallback += handler,
                    handler => device.IlluminanceCallback -= handler)
                    .Select(evt => evt.EventArgs)
                    .Finally(() =>
                    {
                        try { device.SetIlluminanceCallbackConfiguration(0, false, 'x', 0, 1); }
                        catch (NotConnectedException) { } // best effort
                    });
            });
        }
    }

    /// <summary>
    /// Specifies the illuminance range for the Ambient Light Bricklet 3.0.
    /// Smaller illuminance ranges increase the resolution of the data.
    /// </summary>
    public enum AmbientLightV3IlluminanceRange : byte
    {
        /// <summary>
        /// Specifies an illuminance range of 0-64000lux.
        /// </summary>
        Range64000Lux = BrickletAmbientLightV3.ILLUMINANCE_RANGE_64000LUX,

        /// <summary>
        /// Specifies an illuminance range of 0-32000lux.
        /// </summary>
        Range32000Lux = BrickletAmbientLightV3.ILLUMINANCE_RANGE_32000LUX,

        /// <summary>
        /// Specifies an illuminance range of 0-16000lux.
        /// </summary>
        Range16000Lux = BrickletAmbientLightV3.ILLUMINANCE_RANGE_16000LUX,

        /// <summary>
        /// Specifies an illuminance range of 0-8000lux.
        /// </summary>
        Range8000Lux = BrickletAmbientLightV3.ILLUMINANCE_RANGE_8000LUX,

        /// <summary>
        /// Specifies an illuminance range of 0-1300lux.
        /// </summary>
        Range1300Lux = BrickletAmbientLightV3.ILLUMINANCE_RANGE_1300LUX,

        /// <summary>
        /// Specifies an illuminance range of 0-600lux.
        /// </summary>
        Range600Lux = BrickletAmbientLightV3.ILLUMINANCE_RANGE_600LUX,

        /// <summary>
        /// Specifies an illuminance range of 0-~100000lux.
        /// </summary>
        RangeUnlimited = BrickletAmbientLightV3.ILLUMINANCE_RANGE_UNLIMITED
    }

    /// <summary>
    /// Specifies the integration time for the Ambient Light Bricklet 3.0.
    /// Larger integration times result in less noise in the data.
    /// </summary>
    public enum AmbientLightV3IntegrationTime : byte
    {
        /// <summary>
        /// Specifies an integration time of 50ms.
        /// </summary>
        Integration50ms = BrickletAmbientLightV3.INTEGRATION_TIME_50MS,

        /// <summary>
        /// Specifies an integration time of 100ms.
        /// </summary>
        Integration100ms = BrickletAmbientLightV3.INTEGRATION_TIME_100MS,

        /// <summary>
        /// Specifies an integration time of 150ms.
        /// </summary>
        Integration150ms = BrickletAmbientLightV3.INTEGRATION_TIME_150MS,

        /// <summary>
        /// Specifies an integration time of 200ms.
        /// </summary>
        Integration200ms = BrickletAmbientLightV3.INTEGRATION_TIME_200MS,

        /// <summary>
        /// Specifies an integration time of 250ms.
        /// </summary>
        Integration250ms = BrickletAmbientLightV3.INTEGRATION_TIME_250MS,

        /// <summary>
        /// Specifies an integration time of 300ms.
        /// </summary>
        Integration300ms = BrickletAmbientLightV3.INTEGRATION_TIME_300MS,

        /// <summary>
        /// Specifies an integration time of 350ms.
        /// </summary>
        Integration350ms = BrickletAmbientLightV3.INTEGRATION_TIME_350MS,

        /// <summary>
        /// Specifies an integration time of 400ms.
        /// </summary>
        Integration400ms = BrickletAmbientLightV3.INTEGRATION_TIME_400MS,
    }

    /// <summary>
    /// Specifies the behavior of the Ambient Light Bricklet 3.0. status LED.
    /// </summary>
    public enum AmbientLightV3StatusLedConfig : byte
    {
        /// <summary>
        /// The status LED will be permanently OFF.
        /// </summary>
        Off = BrickletAmbientLightV3.STATUS_LED_CONFIG_OFF,

        /// <summary>
        /// The status LED will be permanently ON as long as the bricklet is powered.
        /// </summary>
        On = BrickletAmbientLightV3.STATUS_LED_CONFIG_ON,

        /// <summary>
        /// The status LED will change state periodically every second.
        /// </summary>
        ShowHeartbeat = BrickletAmbientLightV3.STATUS_LED_CONFIG_SHOW_HEARTBEAT,

        /// <summary>
        /// The LED will show communication traffic between Brick and Bricklet,
        /// flickering once for every 10 received data packets.
        /// </summary>
        ShowStatus = BrickletAmbientLightV3.STATUS_LED_CONFIG_SHOW_STATUS
    }
}
