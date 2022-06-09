﻿using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    [Combinator]
    [DefaultProperty(nameof(Uid))]
    [Description("Measures temperature using thermocouple wires from a Thermocouple Bricklet 2.0.")]
    public class ThermocoupleV2
    {
        [TypeConverter(typeof(UidConverter))]
        [Description("The unique bricklet device UID.")]
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
                var device = new BrickletThermocoupleV2(Uid, connection);
                connection.Connected += (sender, e) =>
                {
                    device.SetStatusLEDConfig((byte)StatusLed);
                    device.SetConfiguration((byte)Averaging, (byte)Type, (byte)Filter);
                    device.SetTemperatureCallbackConfiguration(Period, false, 'x', 0, 1);
                };

                return Observable.FromEventPattern<BrickletThermocoupleV2.TemperatureEventHandler, int>(
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
            Averaging1 = BrickletThermocoupleV2.AVERAGING_1,
            Averaging2 = BrickletThermocoupleV2.AVERAGING_2,
            Averaging4 = BrickletThermocoupleV2.AVERAGING_4,
            Averaging8 = BrickletThermocoupleV2.AVERAGING_8,
            Averaging16 = BrickletThermocoupleV2.AVERAGING_16
        }

        public enum ThermocoupleType : byte
        {
            TypeB = BrickletThermocoupleV2.TYPE_B,
            TypeE = BrickletThermocoupleV2.TYPE_E,
            TypeJ = BrickletThermocoupleV2.TYPE_J,
            TypeK = BrickletThermocoupleV2.TYPE_K,
            TypeN = BrickletThermocoupleV2.TYPE_N,
            TypeR = BrickletThermocoupleV2.TYPE_R,
            TypeS = BrickletThermocoupleV2.TYPE_S,
            TypeT = BrickletThermocoupleV2.TYPE_T,
            TypeG8 = BrickletThermocoupleV2.TYPE_G8,
            TypeG32 = BrickletThermocoupleV2.TYPE_G32
        }

        public enum FilterOption : byte
        {
            Filter50Hz = BrickletThermocoupleV2.FILTER_OPTION_50HZ,
            Filter60Hz = BrickletThermocoupleV2.FILTER_OPTION_60HZ
        }

        public enum BrickletThermocoupleV2StatusLedConfig : byte
        {
            Off = BrickletThermocoupleV2.STATUS_LED_CONFIG_OFF,
            On = BrickletThermocoupleV2.STATUS_LED_CONFIG_ON,
            ShowHeartbeat = BrickletThermocoupleV2.STATUS_LED_CONFIG_SHOW_HEARTBEAT,
            ShowStatus = BrickletThermocoupleV2.STATUS_LED_CONFIG_SHOW_STATUS
        }
    }
}