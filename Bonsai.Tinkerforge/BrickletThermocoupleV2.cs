using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    [Combinator]
    [DefaultProperty(nameof(Uid))]
    [Description("Measures temperature using thermocouple wires from a Thermocouple Bricklet 2.0.")]
    public class BrickletThermocoupleV2
    {
        [Description("The unique bricklet device UID.")]
        [TypeConverter(typeof(UidConverter))]
        public string Uid { get; set; }

        [Description("Specifies the period between sample event callbacks. A value of zero disables event reporting.")]
        public long Period { get; set; } = 1000;

        [Description("Specifies the averaging size. This setting will affect conversion time.")]
        public AveragingSize Averaging { get; set; } = AveragingSize.Averaging16;

        [Description("Specifies the type of the thermocouple. Different thermocouple types use different types of metals for their wires.")]
        public ThermocoupleType Type { get; set; } = ThermocoupleType.TypeK;

        [Description("Specifies the filter frequency. Should be configured according to utility frequency.")]
        public FilterOption Filter { get; set; } = FilterOption.Filter50Hz;

        [Description("Specifies the behavior of the status LED.")]
        public BrickletThermocoupleV2StatusLedConfig StatusLed { get; set; } = BrickletThermocoupleV2StatusLedConfig.ShowStatus;

        public IObservable<int> Process(IObservable<IPConnection> source)
        {
            return source.SelectStream(connection =>
            {
                var device = new global::Tinkerforge.BrickletThermocoupleV2(Uid, connection);
                connection.Connected += (sender, e) =>
                {
                    device.SetStatusLEDConfig((byte)StatusLed);
                    device.SetConfiguration((byte)Averaging, (byte)Type, (byte)Filter);
                    device.SetTemperatureCallbackConfiguration(Period, false, 'x', 0, 1);
                };

                return Observable.FromEventPattern<global::Tinkerforge.BrickletThermocoupleV2.TemperatureEventHandler, int>(
                    handler => device.TemperatureCallback += handler,
                    handler => device.TemperatureCallback -= handler)
                    .Select(evt => evt.EventArgs)
                    .Finally(() =>
                    {
                        try { device.SetTemperatureCallbackConfiguration(0, false, 'x', 0, 1); }
                        catch (NotConnectedException) { } // best effort
                });
            });
        }

        public enum AveragingSize : byte
        {
            Averaging1 = global::Tinkerforge.BrickletThermocoupleV2.AVERAGING_1,
            Averaging2 = global::Tinkerforge.BrickletThermocoupleV2.AVERAGING_2,
            Averaging4 = global::Tinkerforge.BrickletThermocoupleV2.AVERAGING_4,
            Averaging8 = global::Tinkerforge.BrickletThermocoupleV2.AVERAGING_8,
            Averaging16 = global::Tinkerforge.BrickletThermocoupleV2.AVERAGING_16
        }

        public enum ThermocoupleType : byte
        {
            TypeB = global::Tinkerforge.BrickletThermocoupleV2.TYPE_B,
            TypeE = global::Tinkerforge.BrickletThermocoupleV2.TYPE_E,
            TypeJ = global::Tinkerforge.BrickletThermocoupleV2.TYPE_J,
            TypeK = global::Tinkerforge.BrickletThermocoupleV2.TYPE_K,
            TypeN = global::Tinkerforge.BrickletThermocoupleV2.TYPE_N,
            TypeR = global::Tinkerforge.BrickletThermocoupleV2.TYPE_R,
            TypeS = global::Tinkerforge.BrickletThermocoupleV2.TYPE_S,
            TypeT = global::Tinkerforge.BrickletThermocoupleV2.TYPE_T,
            TypeG8 = global::Tinkerforge.BrickletThermocoupleV2.TYPE_G8,
            TypeG32 = global::Tinkerforge.BrickletThermocoupleV2.TYPE_G32
        }

        public enum FilterOption : byte
        {
            Filter50Hz = global::Tinkerforge.BrickletThermocoupleV2.FILTER_OPTION_50HZ,
            Filter60Hz = global::Tinkerforge.BrickletThermocoupleV2.FILTER_OPTION_60HZ
        }

        public enum BrickletThermocoupleV2StatusLedConfig : byte
        {
            Off = global::Tinkerforge.BrickletThermocoupleV2.STATUS_LED_CONFIG_OFF,
            On = global::Tinkerforge.BrickletThermocoupleV2.STATUS_LED_CONFIG_ON,
            ShowHeartbeat = global::Tinkerforge.BrickletThermocoupleV2.STATUS_LED_CONFIG_SHOW_HEARTBEAT,
            ShowStatus = global::Tinkerforge.BrickletThermocoupleV2.STATUS_LED_CONFIG_SHOW_STATUS
        }
    }
}
