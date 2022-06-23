using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    [Combinator]
    [Description("Writes an analog output signal (int x.xxx, 3300 = 3.3V, 0-10V range) to an Industrial Analog OUT Bricklet 2.0.")]
    public class IndustrialAnalogOutV2
    {
        [TypeConverter(typeof(UidConverter))]
        [Description("The unique bricklet device UID.")]
        public string Uid { get; set; }

        [Description("Specifies the output voltage range.")]
        public VoltageRangeConfig VoltageRange { get; set; } = VoltageRangeConfig.Range0To10V;

        [Description("Specifies the output current range.")]
        public CurrentRangeConfig CurrentRange { get; set; } = CurrentRangeConfig.Range0To24mA;

        [Description("Specifies the behavior of the Out LED.")]
        public OutLedConfig OutLed { get; set; } = OutLedConfig.ShowOutStatus;

        [Description("Specifies the behavior of the Out Status LED.")]
        public OutLedStatusConfig OutLedStatus { get; set; } = OutLedStatusConfig.Intensity;

        [Description("Status LED minimum value.")]
        public int OutLedStatusMin { get; set; } = 0;

        [Description("Status LED maximum value.")]
        public int OutLedStatusMax { get; set; } = 24000;

        [Description("Specifies the behavior of the status LED.")]
        public BrickletIndustrialAnalogOutV2StatusLedConfig StatusLed { get; set; } = BrickletIndustrialAnalogOutV2StatusLedConfig.ShowStatus;

        [Description("Sets the initial voltage on initialisation. If a negative value is provided the voltage will not be changed on initialisation.")]
        public int? InitialVoltage { get; set; };

        public override string ToString()
        {
            return BrickletIndustrialAnalogOutV2.DEVICE_DISPLAY_NAME;
        }

        public IObservable<int> Process(IObservable<IPConnection> source)
        {
            return Process(source, Observable.Never<int>());
        }

        public IObservable<int> Process(IObservable<IPConnection> source, IObservable<int> signal)
        {
            return source.SelectStream(connection =>
            {
                var device = new BrickletAnalogOutV3(Uid, connection);
                connection.Connected += (sender, e) =>
                {
                    device.SetStatusLEDConfig((byte)StatusLed);
                };

                return Observable.Create<BrickletAnalogOutV3>(observer =>
                {
                    observer.OnNext(device);
                    return Disposable.Create(() =>
                    {
                        try { device.SetStatusLEDConfig(0); }
                        catch (NotConnectedException) { }
                    });
                });
            }).SelectMany(device =>
            {
                var voltage = signal;
                var initialVoltage = InitialVoltage;
                if (initialVoltage.HasValue)
                {
                    voltage = Observable.Return(initialVoltage.Value).Concat(signal);
                }
                return voltage.Do(device.SetOutputVoltage);
            });
        }

        public enum VoltageRangeConfig : byte
        {
            Range0To5V = BrickletIndustrialAnalogOutV2.VOLTAGE_RANGE_0_TO_5V,
            Range0To10V = BrickletIndustrialAnalogOutV2.VOLTAGE_RANGE_0_TO_10V,
        }

        public enum CurrentRangeConfig : byte
        {
            Range4To20mA = BrickletIndustrialAnalogOutV2.CURRENT_RANGE_4_TO_20MA,
            Range0To20mA = BrickletIndustrialAnalogOutV2.CURRENT_RANGE_0_TO_20MA,
            Range0To24mA = BrickletIndustrialAnalogOutV2.CURRENT_RANGE_0_TO_24MA
        }

        public enum OutLedStatusConfig : byte
        {
            Threshold = BrickletIndustrialAnalogOutV2.OUT_LED_STATUS_CONFIG_THRESHOLD,
            Intensity = BrickletIndustrialAnalogOutV2.OUT_LED_STATUS_CONFIG_INTENSITY
        }

        public enum OutLedConfig : byte {
            Off = BrickletIndustrialAnalogOutV2.OUT_LED_CONFIG_OFF,
            On = BrickletIndustrialAnalogOutV2.OUT_LED_CONFIG_ON,
            ShowHeartbeat = BrickletIndustrialAnalogOutV2.OUT_LED_CONFIG_SHOW_HEARTBEAT,
            ShowOutStatus = BrickletIndustrialAnalogOutV2.OUT_LED_CONFIG_SHOW_OUT_STATUS
        }

        public enum BrickletIndustrialAnalogOutV2StatusLedConfig : byte
        {
            Off = BrickletIndustrialAnalogOutV2.STATUS_LED_CONFIG_OFF,
            On = BrickletIndustrialAnalogOutV2.STATUS_LED_CONFIG_ON,
            ShowHeartbeat = BrickletIndustrialAnalogOutV2.STATUS_LED_CONFIG_SHOW_HEARTBEAT,
            ShowStatus = BrickletIndustrialAnalogOutV2.STATUS_LED_CONFIG_SHOW_STATUS
        }
    }
}
