using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    [DefaultProperty(nameof(Uid))]
    [Description("Measures relative humidity from a Humidity Bricklet 2.0.")]
    public class BrickletHumidityV2 : Combinator<IPConnection, int>
    {
        [Description("The unique bricklet device UID.")]
        [TypeConverter(typeof(UidConverter))]
        public string Uid { get; set; }

        [Description("Specifies the period between sample event callbacks. A value of zero disables event reporting.")]
        public long Period { get; set; } = 1000;

        [Description("Specifies the heater configuration. The heater can be used to dry the sensor in extremely wet conditions")]

        public HeaterConfig Heater { get; set; } = HeaterConfig.Disabled;

        [Range(1, 1000)]
        [Description("Specifies the moving average window length for humidity. A value of 1 turns off averaging.")]
        public int MovingAverageLengthTemperature { get; set; } = 5;

        [Range(1, 1000)]
        [Description("Specifies the moving average window length for resitance. A value of 1 turns off averaging.")]
        public int MovingAverageLengthHumidity { get; set; } = 5;

        [Description("Specifies the behavior of the status LED.")]
        public BrickletHumidityStatusLedConfig StatusLed { get; set; } = BrickletHumidityStatusLedConfig.ShowStatus;

        public override IObservable<int> Process(IObservable<IPConnection> source)
        {
            return source.SelectStream(connection =>
            {
                var device = new global::Tinkerforge.BrickletHumidityV2(Uid, connection);
                connection.Connected += (sender, e) =>
                {
                    device.SetStatusLEDConfig((byte)StatusLed);
                    device.SetHeaterConfiguration((byte)Heater);
                    device.SetMovingAverageConfiguration(MovingAverageLengthHumidity, MovingAverageLengthTemperature);
                    device.SetHumidityCallbackConfiguration(Period, false, 'x', 0, 0);
                };

                return Observable.Create<int>(observer =>
                {
                    global::Tinkerforge.BrickletHumidityV2.HumidityEventHandler handler = (sender, humidity) =>
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

        public enum HeaterConfig : byte
        {
            Disabled = global::Tinkerforge.BrickletHumidityV2.HEATER_CONFIG_DISABLED,
            Enabled = global::Tinkerforge.BrickletHumidityV2.HEATER_CONFIG_ENABLED
        }

        public enum BrickletHumidityStatusLedConfig : byte
        {
            Off = global::Tinkerforge.BrickletHumidityV2.STATUS_LED_CONFIG_OFF,
            On = global::Tinkerforge.BrickletHumidityV2.STATUS_LED_CONFIG_ON,
            ShowHeartbeat = global::Tinkerforge.BrickletHumidityV2.STATUS_LED_CONFIG_SHOW_HEARTBEAT,
            ShowStatus = global::Tinkerforge.BrickletHumidityV2.STATUS_LED_CONFIG_SHOW_STATUS
        }
    }
}
