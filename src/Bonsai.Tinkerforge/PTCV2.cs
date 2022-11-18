﻿using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    /// <summary>
    /// Represents an operator that measures temperature with Pt100 and Pt1000 sensors
    /// from a PTC Bricklet 2.0.
    /// </summary>
    [DefaultProperty(nameof(Uid))]
    public class PTCV2 : Combinator<IPConnection, int>
    {
        /// <summary>
        /// Gets or sets the bricklet device UID.
        /// </summary>
        [TypeConverter(typeof(UidConverter))]
        [DeviceType(typeof(BrickletPTCV2))]
        [Description("The bricklet device UID.")]
        public string Uid { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the period between sample event callbacks.
        /// A value of zero disables event reporting.
        /// </summary>
        [Description("Specifies the period between sample event callbacks. A value of zero disables event reporting.")]
        public long Period { get; set; } = 1000;

        /// <summary>
        /// Gets or sets a value specifying the moving average window length for temperature.
        /// A value of 1 turns off averaging.
        /// </summary>
        [Range(1, 1000)]
        [Description("Specifies the moving average window length for temperature. A value of 1 turns off averaging.")]
        public int MovingAverageLengthTemperature { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value specifying the moving average window length for resistance.
        /// A value of 1 turns off averaging.
        /// </summary>
        [Range(1, 1000)]
        [Description("Specifies the moving average window length for resistance. A value of 1 turns off averaging.")]
        public int MovingAverageLengthResistance { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value specifying the wire mode of the sensor.
        /// </summary>
        [Description("Specifies the wire mode of the sensor.")]
        public PTCV2WireMode WireMode { get; set; } = PTCV2WireMode.WireMode2;

        /// <summary>
        /// Gets or sets a value specifying the behavior of the status LED.
        /// </summary>
        [Description("Specifies the behavior of the status LED.")]
        public PTCV2StatusLedConfig StatusLed { get; set; } = PTCV2StatusLedConfig.ShowStatus;

        /// <inheritdoc/>
        public override string ToString()
        {
            return BrickletPTCV2.DEVICE_DISPLAY_NAME;
        }

        /// <summary>
        /// Measures temperature from a PTC Bricklet 2.0.
        /// </summary>
        /// <param name="source">
        /// A sequence containing the TCP/IP connection to the Brick Daemon.
        /// </param>
        /// <returns>the 
        /// A sequence of <see cref="int"/> values representing the
        /// temperature measurements from the PTC Bricklet 2.0.
        /// in 1/100ths of a degree celsius.
        /// </returns>
        public override IObservable<int> Process(IObservable<IPConnection> source)
        {
            if (string.IsNullOrEmpty(Uid))
            {
                throw new ArgumentException("A device Uid must be specified", "Uid");
            }

            return source.SelectStream(
                connection => new BrickletPTCV2(Uid, connection),
                device =>
                {
                    device.SetStatusLEDConfig((byte)StatusLed);
                    device.SetWireMode((byte)WireMode);
                    device.SetMovingAverageConfiguration(MovingAverageLengthResistance, MovingAverageLengthTemperature);
                    device.SetTemperatureCallbackConfiguration(Period, false, 'x', 0, 0);

                    return Observable.FromEvent<BrickletPTCV2.TemperatureEventHandler, int>(
                        onNext => (sender, temperature) =>
                        {
                            onNext(temperature);
                        },
                        handler => device.TemperatureCallback += handler,
                        handler => device.TemperatureCallback -= handler)
                        .Finally(() =>
                        {
                            try { device.SetTemperatureCallbackConfiguration(0, false, 'x', 0, 0); }
                            catch (NotConnectedException) { }
                        });
                });
        }

        /// <summary>
        /// Specifies the wire mode configuration of a PTC Bricklet 2.0.
        /// corresponding the wires of the sensor.
        /// </summary>
        public enum PTCV2WireMode : byte
        {
            /// <summary>
            /// The bricklet will use a 2-wire sensor.
            /// </summary>
            WireMode2 = BrickletPTCV2.WIRE_MODE_2,

            /// <summary>
            /// The bricklet will use a 2-wire sensor.
            /// </summary>
            WireMode3 = BrickletPTCV2.WIRE_MODE_3,

            /// <summary>
            /// The bricklet will use a 4-wire sensor.
            /// </summary>
            WireMode4 = BrickletPTCV2.WIRE_MODE_4,
        }

        /// <summary>
        /// Specifies the behavior of the PTC Bricklet 2.0. status LED.
        /// </summary>
        public enum PTCV2StatusLedConfig : byte
        {
            /// <summary>
            /// The status LED will be permanently OFF.
            /// </summary>
            Off = BrickletPTCV2.STATUS_LED_CONFIG_OFF,

            /// <summary>
            /// The status LED will be permanently ON as long as the bricklet is powered.
            /// </summary>
            On = BrickletPTCV2.STATUS_LED_CONFIG_ON,

            /// <summary>
            /// The status LED will change state periodically every second.
            /// </summary>
            ShowHeartbeat = BrickletPTCV2.STATUS_LED_CONFIG_SHOW_HEARTBEAT,

            /// <summary>
            /// The LED will show communication traffic between Brick and Bricklet,
            /// flickering once for every 10 received data packets.
            /// </summary>
            ShowStatus = BrickletPTCV2.STATUS_LED_CONFIG_SHOW_STATUS
        }
    }
}
