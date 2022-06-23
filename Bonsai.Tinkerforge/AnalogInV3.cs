using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    [DefaultProperty(nameof(Uid))]
    [Description("Measures analog signal from an Analog In Bricklet 3.0.")]
    public class AnalogInV3 : Combinator<IPConnection, int>
    {
        [TypeConverter(typeof(UidConverter))]
        [Description("The unique bricklet device UID.")]
        public string Uid { get; set; }

        [Description("Specifies the period between sample event callbacks. A value of zero disables event reporting.")]
        public long Period { get; set; } = 1000;

        [Description("Specifies the behavior of the status LED.")]
        public BrickletAnalogInV3LedConfig StatusLed { get; set; } = BrickletAnalogInV3LedConfig.ShowStatus;

        public override string ToString()
        {
            return BrickletAnalogInV3.DEVICE_DISPLAY_NAME;
        }
        public override IObservable<int> Process(IObservable<IPConnection> source)
        {
            return source.SelectStream(connection =>
            {
                var device = new BrickletAnalogInV3(Uid, connection);
                connection.Connected += (sender, e) =>
                {
                    device.SetStatusLEDConfig((byte)StatusLed);
                    device.SetVoltageCallbackConfiguration(Period, false, 'x', 0, 0);
                };

                return Observable.Create<int>(observer =>
                {
                    BrickletAnalogInV3.VoltageEventHandler handler = (sender, voltage) =>
                    {
                        observer.OnNext(voltage);
                    };

                    device.VoltageCallback += handler;
                    return Disposable.Create(() =>
                    {
                        try { device.SetVoltageCallbackConfiguration(0, false, 'x', 0, 0); }
                        catch (NotConnectedException) { }
                        device.VoltageCallback -= handler;
                    });
                });
            });
        }

        public enum BrickletAnalogInV3LedConfig : byte
        {
            Off = BrickletHumidityV2.STATUS_LED_CONFIG_OFF,
            On = BrickletHumidityV2.STATUS_LED_CONFIG_ON,
            ShowHeartbeat = BrickletHumidityV2.STATUS_LED_CONFIG_SHOW_HEARTBEAT,
            ShowStatus = BrickletHumidityV2.STATUS_LED_CONFIG_SHOW_STATUS
        }
    }
}
