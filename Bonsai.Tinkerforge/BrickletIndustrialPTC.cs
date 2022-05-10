using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    [DefaultProperty(nameof(Uid))]
    [Description("Measures temperature with Pt100 and Pt1000 sensors from a PTC Industrial Bricklet")]
    public class BrickletIndustrialPTC : Combinator<IPConnection, int>
    {
        [Description("The unique bricklet device UID.")]
        public string Uid { get; set; }

        [Description("Specifies the period between sample event callbacks. A value of zero disables event reporting.")]
        public long Period { get; set; } = 1000;

        [Range(1, 1000)]
        [Description("Specifies the moving average window length for temperature. A value of 1 turns off averaging.")]
        public int MovingAverageLengthTemperature { get; set; } = 1;

        [Range(1, 1000)]
        [Description("Specifies the moving average window length for resitance. A value of 1 turns off averaging.")]
        public int MovingAverageLengthResistance { get; set; } = 1;

        [Description("Specifies the wire mode of the sensor.")]
        public WireModeConfig WireMode { get; set; } = WireModeConfig.WireMode2;

        [Description("Specifies the behavior of the status LED.")]
        public BrickletIndustrialPTCStatusLedConfig StatusLed { get; set; } = BrickletIndustrialPTCStatusLedConfig.ShowStatus;

        public override IObservable<int> Process(IObservable<IPConnection> source)
        {
            return source.SelectStream(connection =>
            {
                var device = new global::Tinkerforge.BrickletIndustrialPTC(Uid, connection);
                connection.Connected += (sender, e) =>
                {
                    device.SetStatusLEDConfig((byte)StatusLed);
                    device.SetWireMode((byte)WireMode);
                    device.SetMovingAverageConfiguration(MovingAverageLengthResistance, MovingAverageLengthTemperature);
                    device.SetTemperatureCallbackConfiguration(Period, false, 'x', 0, 0);
                };

                return Observable.Create<int>(observer =>
                {
                    global::Tinkerforge.BrickletIndustrialPTC.TemperatureEventHandler handler = (sender, temperature) =>
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

        public enum WireModeConfig : byte
        {
            WireMode2 = global::Tinkerforge.BrickletIndustrialPTC.WIRE_MODE_2,
            WireMode3 = global::Tinkerforge.BrickletIndustrialPTC.WIRE_MODE_3,
            WireMode4 = global::Tinkerforge.BrickletIndustrialPTC.WIRE_MODE_4,
        }

        public enum BrickletIndustrialPTCStatusLedConfig : byte
        {
            Off = global::Tinkerforge.BrickletIndustrialPTC.STATUS_LED_CONFIG_OFF,
            On = global::Tinkerforge.BrickletIndustrialPTC.STATUS_LED_CONFIG_ON,
            ShowHeartbeat = global::Tinkerforge.BrickletIndustrialPTC.STATUS_LED_CONFIG_SHOW_HEARTBEAT,
            ShowStatus = global::Tinkerforge.BrickletIndustrialPTC.STATUS_LED_CONFIG_SHOW_STATUS
        }
    }
}
