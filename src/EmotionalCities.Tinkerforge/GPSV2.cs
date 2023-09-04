using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Bonsai;
using Tinkerforge;

namespace EmotionalCities.Tinkerforge
{
    /// <summary>
    /// Represents an operator that generates a device instance for a GPS Bricklet 2.0.
    /// Can be connected to Coordinate, Altitude, DateTime and Status output streams.
    /// </summary>
    [DefaultProperty(nameof(Uid))]
    [Description("Generates a device instance for a GPS Bricklet 2.0. Can be connected to Coordinate, Altitude, DateTime, Status output streams.")]
    public class GPSV2 : Combinator<IPConnection, BrickletGPSV2>
    {
        /// <summary>
        /// Gets or sets the bricklet device UID.
        /// </summary>
        [TypeConverter(typeof(UidConverter))]
        [DeviceType(typeof(BrickletGPSV2))]
        [Description("The bricklet device UID.")]
        public string Uid { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the SBAS (Satellite-Based Augmentation System) configuration.
        /// If SBAS is enabled, the position accuracy increases (if SBAS satellites are in view), but the
        /// update rate is limited to 5Hz. With SBAS disabled the update rate is increased to 10Hz.
        /// </summary>
        [Description("If SBAS (Satellite-based Augmentation System) is enabled, the position accuracy increases (if SBAS satellites are in view), but the update rate is limited to 5Hz. With SBAS disabled the update rate is increased to 10Hz.")]
        public GPSV2SBASConfig SBAS { get; set; } = GPSV2SBASConfig.Disabled;

        /// <summary>
        /// Gets or sets a value specifying the behavior of the status LED.
        /// </summary>
        [Description("Specifies the behavior of the status LED.")]
        public GPSV2StatusLedConfig StatusLed { get; set; } = GPSV2StatusLedConfig.ShowStatus;

        /// <summary>
        /// Gets or sets a value specifying the period between datetime sample event callbacks.
        /// A value of zero disables event reporting.
        /// </summary>
        [Description("Specifies the period between datetime sample event callbacks. A value of zero disables event reporting.")]
        public long DateTimePeriod { get; set; } = 1000;

        /// <summary>
        /// Gets or sets a value specifying the period between altitude sample event callbacks.
        /// A value of zero disables event reporting.
        /// </summary>
        [Description("Specifies the period between altitude sample event callbacks. A value of zero disables event reporting.")]
        public long AltitudePeriod { get; set; } = 1000;

        /// <summary>
        /// Gets or sets a value specifying the period between coordinate sample event callbacks.
        /// A value of zero disables event reporting.
        /// </summary>
        [Description("Specifies the period between coordinate sample event callbacks. A value of zero disables event reporting.")]
        public long CoordinatePeriod { get; set; } = 1000;

        /// <summary>
        /// Gets or sets a value specifying the period between status sample event callbacks.
        /// A value of zero disables event reporting.
        /// </summary>
        [Description("Specifies the period between status sample event callbacks. A value of zero disables event reporting.")]
        public long StatusPeriod { get; set; } = 1000;

        /// <inheritdoc/>
        public override string ToString()
        {
            return BrickletGPSV2.DEVICE_DISPLAY_NAME;
        }

        /// <summary>
        /// Generates a device instance for a GPS Bricklet 2.0.
        /// </summary>
        /// <param name="source">
        /// A sequence containing the TCP/IP connection to the Brick Daemon.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="BrickletGPSV2"/> objects representing a handle 
        /// to the GPS Bricklet 2.0.
        /// </returns>
        public override IObservable<BrickletGPSV2> Process(IObservable<IPConnection> source)
        {
            var uid = UidHelper.ThrowIfNullOrEmpty(Uid);

            return source.SelectStream(
                connection => new BrickletGPSV2(uid, connection),
                device =>
                {
                    device.SetStatusLEDConfig((byte)StatusLed);
                    device.SetSBASConfig((byte)SBAS);
                    device.SetDateTimeCallbackPeriod(DateTimePeriod);
                    device.SetAltitudeCallbackPeriod(AltitudePeriod);
                    device.SetCoordinatesCallbackPeriod(CoordinatePeriod);
                    device.SetStatusCallbackPeriod(StatusPeriod);

                    return Observable.Create<BrickletGPSV2>(observer =>
                    {
                        observer.OnNext(device);
                        return Disposable.Create(() =>
                        {
                            try { device.SetStatusLEDConfig(0); }
                            catch (NotConnectedException) { }
                        });
                    });
                });
        }
    }

    /// <summary>
    /// Specifies the SBAS configuration.
    /// </summary>
    public enum GPSV2SBASConfig : byte
    {
        /// <summary>
        /// Specifies that SBAS will be enabled.
        /// </summary>
        Enabled = BrickletGPSV2.SBAS_ENABLED,

        /// <summary>
        /// Specifies that SBAS will be disabled.
        /// </summary>
        Disabled = BrickletGPSV2.SBAS_DISABLED,
    }

    /// <summary>
    /// Specifies the behavior of the GPS Bricklet 2.0. status LED.
    /// </summary>
    public enum GPSV2StatusLedConfig : byte
    {
        /// <summary>
        /// The status LED will be permanently OFF.
        /// </summary>
        Off = BrickletGPSV2.STATUS_LED_CONFIG_OFF,

        /// <summary>
        /// The status LED will be permanently ON as long as the bricklet is powered.
        /// </summary>
        On = BrickletGPSV2.STATUS_LED_CONFIG_ON,

        /// <summary>
        /// The status LED will change state periodically every second.
        /// </summary>
        ShowHeartbeat = BrickletGPSV2.STATUS_LED_CONFIG_SHOW_HEARTBEAT,

        /// <summary>
        /// The LED will show communication traffic between Brick and Bricklet,
        /// flickering once for every 10 received data packets.
        /// </summary>
        ShowStatus = BrickletGPSV2.STATUS_LED_CONFIG_SHOW_STATUS
    }
}
