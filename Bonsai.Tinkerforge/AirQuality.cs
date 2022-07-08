using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    /// <summary>
    /// Represents an operator that measures IAQ (indoor air quality) index, air
    /// pressure, humidity and temperature from an Air Quality Bricklet.
    /// </summary>
    [DefaultProperty(nameof(Uid))]
    [Description("Measures IAQ (indoor air quality) index, air pressure, humidity and temperature from an Air Quality Bricklet.")]
    public class AirQuality : Combinator<IPConnection, AirQuality.AirQualityDataFrame>
    {
        /// <summary>
        /// Gets or sets the bricklet device UID.
        /// </summary>
        [TypeConverter(typeof(UidConverter))]
        [Description("The bricklet device UID.")]
        public string Uid { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the period between sample event callbacks.
        /// A value of zero disables event reporting.
        /// </summary>
        [Description("Specifies the period between sample event callbacks. A value of zero disables event reporting.")]
        public long Period { get; set; } = 1000;

        /// <summary>
        /// Gets or sets a value specifying a temperature offset, in hundredths of a
        /// degree, to compensate for heat inside an enclosure.
        /// </summary>
        [Description("Specifies a temperature offset, in hundredths of a degree, to compensate for heat inside an enclosure.")]
        public int TemperatureOffset { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the duration of background calibration
        /// (requires power cycle to change).
        /// </summary>
        [Description("Specifies the duration of background calibration (requires power cycle to change).")]
        public BackgroundCalibrationDurationConfig BackgroundCalibrationDuration { get; set; } = BackgroundCalibrationDurationConfig.Days4;

        /// <summary>
        /// Gets or sets a value specifying the behavior of the status LED.
        /// </summary>
        [Description("Specifies the behavior of the status LED.")]
        public BrickletAirQualityStatusLedConfig StatusLed { get; set; } = BrickletAirQualityStatusLedConfig.ShowStatus;

        /// <inheritdoc/>
        public override string ToString()
        {
            return BrickletAirQuality.DEVICE_DISPLAY_NAME;
        }

        /// <summary>
        /// Measures IAQ (indoor air quality) index, air pressure, humidity and
        /// temperature from an Air Quality Bricklet.
        /// </summary>
        /// <param name="source">
        /// A sequence containing the TCP/IP connection to the Brick Daemon.
        /// </param>
        /// <returns>the 
        /// A sequence of <see cref="AirQualityDataFrame"/> objects representing the
        /// measurements from the Air Quality Bricklet.
        /// </returns>
        public override IObservable<AirQualityDataFrame> Process(IObservable<IPConnection> source)
        {
            return source.SelectStream(connection =>
            {
                var device = new BrickletAirQuality(Uid, connection);
                connection.Connected += (sender, e) => {
                    device.SetStatusLEDConfig((byte)StatusLed);
                    device.SetTemperatureOffset(TemperatureOffset);
                    device.SetBackgroundCalibrationDuration((byte)BackgroundCalibrationDuration);
                    device.SetAllValuesCallbackConfiguration(Period, false);
                };

                return Observable.Create<AirQualityDataFrame>(observer =>
                {
                    BrickletAirQuality.AllValuesEventHandler handler = (sender, iaqIndex, iaqAccuracy, temperature, humidity, airPressure) =>
                    {
                        observer.OnNext(new AirQualityDataFrame(
                            iaqIndex,
                            (BrickletAirQualityAccuracy)iaqAccuracy,
                            temperature,
                            humidity,
                            airPressure));
                    };

                    device.AllValuesCallback += handler;
                    return Disposable.Create(() =>
                    {
                        try { device.SetAllValuesCallbackConfiguration(0, false); }
                        catch (NotConnectedException) { }
                        device.AllValuesCallback -= handler;
                    });
                });
            });
        }

        /// <summary>
        /// Represents a set of measurement values sampled from an Air Quality Bricklet.
        /// </summary>
        public struct AirQualityDataFrame
        {
            /// <summary>
            /// Represents the IAQ (indoor air quality) index, between 0 (best) and 500 (worst).
            /// </summary>
            public int IaqIndex;

            /// <summary>
            /// Represents the IAQ index accuracy.
            /// </summary>
            public BrickletAirQualityAccuracy IaqIndexAccuracy;

            /// <summary>
            /// Represents the current temperature, in hundredths of a degree (celsius).
            /// </summary>
            public int Temperature;

            /// <summary>
            /// Represents the relative humidity in hundredths of percentage points (%RH).
            /// </summary>
            public int Humidity;

            /// <summary>
            /// Represents the current air pressure in hundredths of hPa.
            /// </summary>
            public int AirPressure;

            /// <summary>
            /// Initializes a new instance of the <see cref="AirQualityDataFrame"/> structure.
            /// </summary>
            /// <param name="iaqIndex">The IAQ (indoor air quality) index.</param>
            /// <param name="iaqAccuracy">The IAQ index accuracy.</param>
            /// <param name="temperature">The current temperature value.</param>
            /// <param name="humidity">The current relative humidity value.</param>
            /// <param name="airPressure">The current air pressure value.</param>
            public AirQualityDataFrame(int iaqIndex, BrickletAirQualityAccuracy iaqAccuracy, int temperature, int humidity, int airPressure)
            {
                IaqIndex = iaqIndex; 
                IaqIndexAccuracy = iaqAccuracy;
                Temperature = temperature;
                Humidity = humidity;
                AirPressure = airPressure;
            }
        }

        /// <summary>
        /// Specifies the duration of the background calibration for the Air Quality Bricklet.
        /// </summary>
        public enum BackgroundCalibrationDurationConfig : byte
        {
            /// <summary>
            /// Specifies that the automatic background calibration of the IAQ index
            /// will run for 4 days.
            /// </summary>
            Days4 = BrickletAirQuality.DURATION_4_DAYS,

            /// <summary>
            /// Specifies that the automatic background calibration of the IAQ index
            /// will run for 28 days.
            /// </summary>
            Days28 = BrickletAirQuality.DURATION_28_DAYS
        }

        /// <summary>
        /// Specifies the behavior of the Air Quality Bricklet status LED.
        /// </summary>
        public enum BrickletAirQualityStatusLedConfig : byte
        {
            /// <summary>
            /// The status LED will be permanently OFF.
            /// </summary>
            Off = BrickletAirQuality.STATUS_LED_CONFIG_OFF,

            /// <summary>
            /// The status LED will be permanently ON as long as the bricklet is powered.
            /// </summary>
            On = BrickletAirQuality.STATUS_LED_CONFIG_ON,

            /// <summary>
            /// The status LED will change state periodically every second.
            /// </summary>
            ShowHeartbeat = BrickletAirQuality.STATUS_LED_CONFIG_SHOW_HEARTBEAT,

            /// <summary>
            /// The LED will show communication traffic between Brick and Bricklet,
            /// flickering once for every 10 received data packets.
            /// </summary>
            ShowStatus = BrickletAirQuality.STATUS_LED_CONFIG_SHOW_STATUS
        }

        /// <summary>
        /// Specifies the accuracy of the IAQ index.
        /// </summary>
        public enum BrickletAirQualityAccuracy : byte
        {
            /// <summary>
            /// The Bricklet was just started and the sensor is stabilizing.
            /// </summary>
            Unreliable = 0,

            /// <summary>
            /// The background history is uncertain. This typically means the gas sensor
            /// data was too stable for the calibration algorithm to clearly define its references.
            /// </summary>
            Low = 1,

            /// <summary>
            /// The Bricklet found new calibration data and is currently calibrating.
            /// </summary>
            Medium = 2,

            /// <summary>
            /// The Bricklet is calibrated successfully.
            /// </summary>
            High = 3
        }
    }
}
