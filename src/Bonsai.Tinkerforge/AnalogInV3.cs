using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    /// <summary>
    /// Represents an operator that measures an analog voltage signal
    /// from an Analog In Bricklet 3.0.
    /// </summary>
    [DefaultProperty(nameof(Uid))]
    [Description("Measures an analog voltage signal from an Analog In Bricklet 3.0.")]
    public class AnalogInV3 : Combinator<IPConnection, int>
    {
        /// <summary>
        /// Gets or sets the bricklet device UID.
        /// </summary>
        [TypeConverter(typeof(UidConverter))]
        [DeviceType(typeof(BrickletAnalogInV3))]
        [Description("The bricklet device UID.")]
        public string Uid { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the period between sample event callbacks.
        /// A value of zero disables event reporting.
        /// </summary>
        [Description("Specifies the period between sample event callbacks. A value of zero disables event reporting.")]
        public long Period { get; set; } = 1000;

        /// <summary>
        /// Gets or sets a value specifying the behavior of the status LED.
        /// </summary>
        [Description("Specifies the behavior of the status LED.")]
        public AnalogInV3StatusLedConfig StatusLed { get; set; } = AnalogInV3StatusLedConfig.ShowStatus;

        /// <inheritdoc/>
        public override string ToString()
        {
            return BrickletAnalogInV3.DEVICE_DISPLAY_NAME;
        }

        /// <summary>
        /// Measures an analog voltage signal from an Analog In Bricklet 3.0.
        /// </summary>
        /// <param name="source">
        /// A sequence containing the TCP/IP connection to the Brick Daemon.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="int"/> values representing the
        /// measurements from the Analog In Bricklet 3.0. in units of 1mV.
        /// </returns>
        public override IObservable<int> Process(IObservable<IPConnection> source)
        {
            if (string.IsNullOrEmpty(Uid))
            {
                throw new ArgumentException("A device Uid must be specified", "Uid");
            }

            return source.SelectStream(
                connection => new BrickletAnalogInV3(Uid, connection),
                device =>
                {
                    device.SetStatusLEDConfig((byte)StatusLed);
                    device.SetVoltageCallbackConfiguration(Period, false, 'x', 0, 0);

                    return Observable.FromEvent<BrickletAnalogInV3.VoltageEventHandler, int>(
                        onNext => (sender, voltage) => onNext(voltage),
                        handler => device.VoltageCallback += handler,
                        handler => device.VoltageCallback -= handler)
                        .Finally(() =>
                        {
                            try { device.SetVoltageCallbackConfiguration(0, false, 'x', 0, 0); }
                            catch (NotConnectedException) { }
                        });
                });
        }
    }

    /// <summary>
    /// Specifies the behavior of the Analog In Bricklet 3.0. status LED.
    /// </summary>
    public enum AnalogInV3StatusLedConfig : byte
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
