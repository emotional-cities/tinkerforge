using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using Tinkerforge;
using Bonsai;

namespace EmotionalCities.Tinkerforge
{
    /// <summary>
    /// Represents an operator that writes an analog output voltage signal to an Analog Out Bricklet 3.0.
    /// </summary>
    [Combinator]
    [DefaultProperty(nameof(Uid))]
    [Description("Writes an analog output voltage signal to an Analog Out Bricklet 3.0.")]
    public class AnalogOutV3
    {
        /// <summary>
        /// Gets or sets the bricklet device UID.
        /// </summary>
        [TypeConverter(typeof(UidConverter))]
        [DeviceType(typeof(BrickletAnalogOutV3))]
        [Description("The bricklet device UID.")]
        public string Uid { get; set; }

        /// <summary>
        /// Gets or sets the initial voltage on initialisation. 
        /// If a value is not provided the voltage will not change on initialisation.
        /// </summary>
        [Description("Sets the initial voltage on initialisation. If a value is not provided the voltage will not be changed on initialisation.")]
        public int? InitialVoltage { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the behavior of the status LED.
        /// </summary>
        [Description("Specifies the behavior of the status LED.")]
        public AnalogOutV3StatusLedConfig StatusLed { get; set; } = AnalogOutV3StatusLedConfig.ShowStatus;

        /// <inheritdoc/>
        public override string ToString()
        {
            return BrickletAnalogOutV3.DEVICE_DISPLAY_NAME;
        }

        /// <summary>
        /// Writes only the initial voltage to an Analog Out Bricklet 3.0.
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
        /// Writes a sequence of voltage values to an Analog Out Bricklet 3.0.
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
    /// Specifies the behavior of the Analog Out Bricklet 3.0. status LED.
    /// </summary>
    public enum AnalogOutV3StatusLedConfig : byte
    {
        /// <summary>
        /// The status LED will be permanently OFF.
        /// </summary>
        Off = BrickletHumidityV2.STATUS_LED_CONFIG_OFF,

        /// <summary>
        /// The status LED will be permanently ON as long as the bricklet is powered.
        /// </summary>
        On = BrickletHumidityV2.STATUS_LED_CONFIG_ON,

        /// <summary>
        /// The status LED will change state periodically every second.
        /// </summary>
        ShowHeartbeat = BrickletHumidityV2.STATUS_LED_CONFIG_SHOW_HEARTBEAT,

        /// <summary>
        /// The LED will show communication traffic between Brick and Bricklet,
        /// flickering once for every 10 received data packets.
        /// </summary>
        ShowStatus = BrickletHumidityV2.STATUS_LED_CONFIG_SHOW_STATUS
    }
}
