using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using Tinkerforge;
using Bonsai;

namespace EmotionalCities.Tinkerforge
{
    /// <summary>
    /// Represents an operator that writes an analog output voltage signal to an Industrial Analog Out Bricklet 2.0.
    /// </summary>
    [Combinator]
    [Description("Writes an analog output voltage signal to an Industrial Analog OUT Bricklet 2.0.")]
    public class IndustrialAnalogOutV2
    {
        /// <summary>
        /// Gets or sets the bricklet device UID.
        /// </summary>
        [TypeConverter(typeof(UidConverter))]
        [DeviceType(typeof(BrickletIndustrialAnalogOutV2))]
        [Description("The bricklet device UID.")]
        public string Uid { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the output voltage range.
        /// </summary>
        [Description("Specifies the output voltage range.")]
        public IndustrialAnalogOutV2VoltageRange VoltageRange { get; set; } = IndustrialAnalogOutV2VoltageRange.Range0To10V;

        /// <summary>
        /// Gets or sets a value specifying the output current range.
        /// </summary>
        [Description("Specifies the output current range.")]
        public IndustrialAnalogOutV2CurrentRange CurrentRange { get; set; } = IndustrialAnalogOutV2CurrentRange.Range0To24mA;

        /// <summary>
        /// Gets or sets a value specifying the behaviour of the 'out' LED
        /// </summary>
        [Description("Specifies the behavior of the Out LED.")]
        public IndustrialAnalogOutV2OutLedConfig OutLed { get; set; } = IndustrialAnalogOutV2OutLedConfig.ShowOutStatus;

        /// <summary>
        /// Gets or sets a value specifying the behavior of the out status LED.
        /// </summary>
        [Description("Specifies the behavior of the Out Status LED.")]
        public IndustrialAnalogOutV2OutLedStatusConfig OutLedStatus { get; set; } = IndustrialAnalogOutV2OutLedStatusConfig.Intensity;

        /// <summary>
        /// Gets or sets the minimum value of the status LED.
        /// </summary>
        [Description("Status LED minimum value.")]
        public int OutLedStatusMin { get; set; } = 0;

        /// <summary>
        /// Gets or sets maximum value of the status LED.
        /// </summary>
        [Description("Status LED maximum value.")]
        public int OutLedStatusMax { get; set; } = 24000;

        /// <summary>
        /// Gets or sets a value specifying the behavior of the status LED.
        /// </summary>
        [Description("Specifies the behavior of the status LED.")]
        public IndustrialAnalogOutV2StatusLedConfig StatusLed { get; set; } = IndustrialAnalogOutV2StatusLedConfig.ShowStatus;

        /// <summary>
        /// Gets or sets a value specifying the initial voltage on initialisation.
        /// If a value is not provided the voltage will not be changed on initialisation.
        /// </summary>
        [Description("Sets the initial voltage on initialisation. If a value is not provided the voltage will not be changed on initialisation.")]
        public int? InitialVoltage { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return BrickletIndustrialAnalogOutV2.DEVICE_DISPLAY_NAME;
        }

        /// <summary>
        /// Writes only the initial voltage to an Industrial Analog Out Bricklet 2.0.
        /// </summary>
        /// <param name="source">
        /// A sequence containing the TCP/IP connection to the Brick Daemon.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="int"/> values representing the
        /// written voltage in units of 1mV.
        /// </returns>
        public IObservable<int> Process(IObservable<IPConnection> source)
        {
            return Process(source, Observable.Never<int>());
        }

        /// <summary>
        /// Writes a sequence of voltage values to an Industrial Analog Out Bricklet 2.0.
        /// </summary>
        /// <param name="source">
        /// A sequence containing the TCP/IP connection to the Brick Daemon.
        /// </param>
        /// <param name="signal">
        /// A sequence of <see cref="int"/> values to write as voltages to the analog out in units of 1mV.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="int"/> values representing the
        /// written voltage in units of 1mV.
        /// </returns>
        public IObservable<int> Process(IObservable<IPConnection> source, IObservable<int> signal)
        {
            var uid = UidHelper.ThrowIfNullOrEmpty(Uid);

            return source.SelectStream(
                connection => new BrickletAnalogOutV3(uid, connection),
                device =>
                {
                    device.SetStatusLEDConfig((byte)StatusLed);

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
    }

    /// <summary>
    /// Specifies the voltage range configuration of the Industrial Analog Out Bricklet 2.0.
    /// The resolution will always be 12 bit, therefore precision is higher with a smaller range.
    /// </summary>
    public enum IndustrialAnalogOutV2VoltageRange : byte
    {
        /// <summary>
        /// The voltage range will be 0-5V.
        /// </summary>
        Range0To5V = BrickletIndustrialAnalogOutV2.VOLTAGE_RANGE_0_TO_5V,

        /// <summary>
        /// The voltage range will be 0-10V.
        /// </summary>
        Range0To10V = BrickletIndustrialAnalogOutV2.VOLTAGE_RANGE_0_TO_10V,
    }

    /// <summary>
    /// Specifies the current range configuration of the Industrial Analog Out Bricklet 2.0.
    /// The resolution will always be 12 bit, therefore precision is higher with a smaller range.
    /// </summary>
    public enum IndustrialAnalogOutV2CurrentRange : byte
    {
        /// <summary>
        /// The current range will be 4-20mA.
        /// </summary>
        Range4To20mA = BrickletIndustrialAnalogOutV2.CURRENT_RANGE_4_TO_20MA,

        /// <summary>
        /// The current range will be 0-20mA.
        /// </summary>
        Range0To20mA = BrickletIndustrialAnalogOutV2.CURRENT_RANGE_0_TO_20MA,

        /// <summary>
        /// The current range will be 0-24mA.
        /// </summary>
        Range0To24mA = BrickletIndustrialAnalogOutV2.CURRENT_RANGE_0_TO_24MA
    }

    /// <summary>
    /// Specifies the configuration of the Industrial Analog Out Bricklet 2.0. out status LED.
    /// </summary>
    public enum IndustrialAnalogOutV2OutLedStatusConfig : byte
    {
        /// <summary>
        /// The out status LED will function in threshold mode.
        /// A positive or negative threshold can be set above or below which 
        /// the LED will turn on.
        /// </summary>
        Threshold = BrickletIndustrialAnalogOutV2.OUT_LED_STATUS_CONFIG_THRESHOLD,

        /// <summary>
        /// The out status LED will function in intensity mode.
        /// The LED brightness will scale with a mV or uA value.
        /// </summary>
        Intensity = BrickletIndustrialAnalogOutV2.OUT_LED_STATUS_CONFIG_INTENSITY
    }

    /// <summary>
    /// Specifies the configuration of the Industrial Analog Out Bricklet 2.0. out LED.
    /// </summary>
    public enum IndustrialAnalogOutV2OutLedConfig : byte
    {
        /// <summary>
        /// The out LED will be permanently OFF.
        /// </summary>
        Off = BrickletIndustrialAnalogOutV2.OUT_LED_CONFIG_OFF,

        /// <summary>
        /// The out LED will be permanently ON as long as the bricklet is powered.
        /// </summary>
        On = BrickletIndustrialAnalogOutV2.OUT_LED_CONFIG_ON,

        /// <summary>
        /// The out LED will change state periodically every second.
        /// </summary>
        ShowHeartbeat = BrickletIndustrialAnalogOutV2.OUT_LED_CONFIG_SHOW_HEARTBEAT,

        /// <summary>
        /// The out LED will show communication traffic between Brick and Bricklet,
        /// flickering once for every 10 received data packets.
        /// </summary>
        ShowOutStatus = BrickletIndustrialAnalogOutV2.OUT_LED_CONFIG_SHOW_OUT_STATUS
    }

    /// <summary>
    /// Specifies the behavior of the Industrial Analog Out Bricklet 2.0. status LED.
    /// </summary>
    public enum IndustrialAnalogOutV2StatusLedConfig : byte
    {
        /// <summary>
        /// The status LED will be permanently OFF.
        /// </summary>
        Off = BrickletIndustrialAnalogOutV2.STATUS_LED_CONFIG_OFF,

        /// <summary>
        /// The status LED will be permanently ON as long as the bricklet is powered.
        /// </summary>
        On = BrickletIndustrialAnalogOutV2.STATUS_LED_CONFIG_ON,

        /// <summary>
        /// The status LED will change state periodically every second.
        /// </summary>
        ShowHeartbeat = BrickletIndustrialAnalogOutV2.STATUS_LED_CONFIG_SHOW_HEARTBEAT,

        /// <summary>
        /// The LED will show communication traffic between Brick and Bricklet,
        /// flickering once for every 10 received data packets.
        /// </summary>
        ShowStatus = BrickletIndustrialAnalogOutV2.STATUS_LED_CONFIG_SHOW_STATUS
    }
}
