using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    [DefaultProperty(nameof(Uid))]
    [Description("Measure IAQ (indoor air quality) index, air pressure, humidity and temperature from an Air Quality Bricklet.")]
    public class AirQuality : Combinator<IPConnection, AirQuality.AirQualityDataFrame>
    {
        [TypeConverter(typeof(UidConverter))]
        [Description("The unique bricklet device UID.")]
        public string Uid { get; set; }

        [Description("Specifies the period between sample event callbacks. A value of zero disables event reporting.")]
        public long Period { get; set; } = 1000;

        [Description("Specifies a temperature offset, in hundredths of a degree, to compensate for heat inside an enclosure.")]
        public int TemperatureOffset { get; set; }

        [Description("Specifies the duration of background calibration (requires power cycle to change).")]
        public BackgroundCalibrationDurationConfig BackgroundCalibrationDuration { get; set; } = BackgroundCalibrationDurationConfig.Days4;

        [Description("Specifies the behavior of the status LED.")]
        public BrickletAirQualityStatusLedConfig StatusLed { get; set; } = BrickletAirQualityStatusLedConfig.ShowStatus;

        public override string ToString()
        {
            return BrickletAirQuality.DEVICE_DISPLAY_NAME;
        }

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
                        observer.OnNext(new AirQualityDataFrame(iaqIndex, iaqAccuracy, temperature, humidity, airPressure));
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

        public struct AirQualityDataFrame
        {
            public int IaqIndex; 
            public byte IaqIndexAccuracy;
            public int Temperature;
            public int Humidity;
            public int AirPressure;

            public AirQualityDataFrame(int iaqIndex, byte iaqAccuracy, int temperature, int humidity, int airPressure)
            {
                IaqIndex = iaqIndex; 
                IaqIndexAccuracy = iaqAccuracy;
                Temperature = temperature;
                Humidity = humidity;
                AirPressure = airPressure;
            }
        }

        public enum BackgroundCalibrationDurationConfig : byte
        {
            Days4 = BrickletAirQuality.DURATION_4_DAYS,
            Days28 = BrickletAirQuality.DURATION_28_DAYS
        }

        public enum BrickletAirQualityStatusLedConfig : byte
        {
            Off = BrickletAirQuality.STATUS_LED_CONFIG_OFF,
            On = BrickletAirQuality.STATUS_LED_CONFIG_ON,
            ShowHeartbeat = BrickletAirQuality.STATUS_LED_CONFIG_SHOW_HEARTBEAT,
            ShowStatus = BrickletAirQuality.STATUS_LED_CONFIG_SHOW_STATUS
        }
    }
}
