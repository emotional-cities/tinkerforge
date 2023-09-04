using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Bonsai;
using Tinkerforge;

namespace EmotionalCities.Tinkerforge
{
    /// <summary>
    /// Represents an operator that measures different sizes of particular matter
    /// from a Particulate Matter Bricklet.
    /// </summary>
    [DefaultProperty(nameof(Uid))]
    [Description("Measures different sizes of particulate matter from a Particulate Matter Bricklet.")]
    public class ParticulateMatter : Combinator<IPConnection, ParticulateMatterDataFrame>
    {
        /// <summary>
        /// Gets or sets the bricklet device UID.
        /// </summary>
        [TypeConverter(typeof(UidConverter))]
        [DeviceType(typeof(BrickletParticulateMatter))]
        [Description("The bricklet device UID.")]
        public string Uid { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the period between sample event callbacks.
        /// A value of zero disables event reporting.
        /// </summary>
        [Description("Specifies the period between sample event callbacks. A value of zero disables event reporting.")]
        public long Period { get; set; } = 1000;

        /// <summary>
        /// Gets or sets a value specifying the behavior of the status LED.
        /// </summary>
        [Description("Specifies the behavior of the status LED.")]
        public ParticulateMatterStatusLedConfig StatusLed { get; set; } = ParticulateMatterStatusLedConfig.ShowStatus;

        /// <inheritdoc/>
        public override string ToString()
        {
            return BrickletParticulateMatter.DEVICE_DISPLAY_NAME;
        }

        /// <summary>
        /// Measures different sizes of particulate matter
        /// from a Particulate Matter Bricklet.
        /// </summary>
        /// <param name="source">
        /// A sequence containing the TCP/IP connection to the Brick Daemon.
        /// </param>
        /// <returns>the 
        /// A sequence of <see cref="ParticulateMatterDataFrame"/> objects representing the
        /// measurements from the Particulate Matter Bricklet.
        /// </returns>
        public override IObservable<ParticulateMatterDataFrame> Process(IObservable<IPConnection> source)
        {
            var uid = UidHelper.ThrowIfNullOrEmpty(Uid);

            return source.SelectStream(
                connection => new BrickletParticulateMatter(uid, connection),
                device =>
                {
                    device.SetStatusLEDConfig((byte)StatusLed);
                    device.SetPMConcentrationCallbackConfiguration(Period, false);

                    return Observable.FromEvent<BrickletParticulateMatter.PMConcentrationEventHandler, ParticulateMatterDataFrame>(
                        onNext => (sender, pm10, pm25, pm100) =>
                        {
                            onNext(new ParticulateMatterDataFrame(pm10, pm25, pm100));
                        },
                        handler => device.PMConcentrationCallback += handler,
                        handler => device.PMConcentrationCallback -= handler)
                        .Finally(() =>
                        {
                            try { device.SetPMConcentrationCallbackConfiguration(0, false); }
                            catch (NotConnectedException) { }
                        });
                });
        }
    }

    /// <summary>
    /// Represents a set of measurement values sampled from a Particulate Matter Bricklet.
    /// </summary>
    public struct ParticulateMatterDataFrame
    {
        /// <summary>
        /// Represents the particulate matter concentration of PM10 in units 1ug/m^3.
        /// </summary>
        public int Pm10;

        /// <summary>
        /// Represents the particulate matter concentration of PM25 in units 1ug/m^3.
        /// </summary>
        public int Pm25;

        /// <summary>
        /// Represents the particulate matter concentration of PM25 in units 1ug/m^3.
        /// </summary>
        public int Pm100;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParticulateMatterDataFrame"/> structure.
        /// </summary>
        /// <param name="pm10">The concentration of PM10.</param>
        /// <param name="pm25">The concentration of PM25.</param>
        /// <param name="pm100">The concentration of PM100.</param>
        public ParticulateMatterDataFrame(int pm10, int pm25, int pm100)
        {
            Pm10 = pm10;
            Pm25 = pm25;
            Pm100 = pm100;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(ParticulateMatterDataFrame)} {{ " +
                $"{nameof(Pm10)} = {Pm10}, " +
                $"{nameof(Pm25)} = {Pm25}, " +
                $"{nameof(Pm100)} = {Pm100} }}";
        }
    }

    /// <summary>
    /// Specifies the behavior of the Particulate Matter Bricklet status LED.
    /// </summary>
    public enum ParticulateMatterStatusLedConfig : byte
    {
        /// <summary>
        /// The status LED will be permanently OFF.
        /// </summary>
        Off = BrickletParticulateMatter.STATUS_LED_CONFIG_OFF,

        /// <summary>
        /// The status LED will be permanently ON as long as the bricklet is powered.
        /// </summary>
        On = BrickletParticulateMatter.STATUS_LED_CONFIG_ON,

        /// <summary>
        /// The status LED will change state periodically every second.
        /// </summary>
        ShowHeartbeat = BrickletParticulateMatter.STATUS_LED_CONFIG_SHOW_HEARTBEAT,

        /// <summary>
        /// The LED will show communication traffic between Brick and Bricklet,
        /// flickering once for every 10 received data packets.
        /// </summary>
        ShowStatus = BrickletParticulateMatter.STATUS_LED_CONFIG_SHOW_STATUS
    }
}
