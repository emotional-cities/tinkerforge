using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    /// <summary>
    /// Represents an operator that measures CO2 concentration, temperature and humidity from a CO2 Bricklet 2.0.
    /// </summary>
    [DefaultProperty(nameof(Uid))]
    [Description("Measures CO2 concentration, temperature, and humidity from a CO2 Bricklet 2.0.")]
    public class CO2V2 : Combinator<IPConnection, CO2V2DataFrame>
    {
        /// <summary>
        /// Gets or sets the bricklet device UID.
        /// </summary>
        [TypeConverter(typeof(UidConverter))]
        [DeviceType(typeof(BrickletCO2V2))]
        [Description("The bricklet device UID.")]
        public string Uid { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the period between sample event callbacks.
        /// A value of zero disables event reporting.
        /// </summary>
        [Description("Specifies the period between sample event callbacks. A value of zero disables event reporting.")]
        public long Period { get; set; } = 1000;

        /// <summary>
        /// Gets or sets a value specifying the ambient air pressure, in units of 1hPa.
        /// Can be used to increase the accuracy of the CO2 Bricklet 2.0.
        /// </summary>
        [Description("Specifies the ambient air pressure, in units of 1hPa. Can be used to increase the accuracy of the CO2 Bricklet 2.0.")]
        public int AirPressure { get; set; }

        /// <summary>
        /// Gets or sets a value specifying a temperature offset, in hundredths of a
        /// degree, to compensate for heat inside an enclosure.
        /// </summary>
        [Description("Specifies a temperature offset, in hundredths of a degree, to compensate for heat inside an enclosure.")]
        public int TemperatureOffset { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the behavior of the status LED.
        /// </summary>
        [Description("Specifies the behavior of the status LED.")]
        public CO2V2StatusLedConfig StatusLed { get; set; } = CO2V2StatusLedConfig.ShowStatus;

        /// <inheritdoc/>
        public override string ToString()
        {
            return BrickletCO2V2.DEVICE_DISPLAY_NAME;
        }

        /// <summary>
        /// Measures CO2 concentration, temperature and humidity from a CO2 Bricklet 2.0.
        /// </summary>
        /// <param name="source">
        /// A sequence containing the TCP/IP connection to the Brick Daemon.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="CO2V2DataFrame"/> objects representing the
        /// measurements from the CO2 Bricklet 2.0.
        /// </returns>
        public override IObservable<CO2V2DataFrame> Process(IObservable<IPConnection> source)
        {
            var uid = UidHelper.ThrowIfNullOrEmpty(Uid);

            return source.SelectStream(
                connection => new BrickletCO2V2(uid, connection),
                device =>
                {
                    device.SetStatusLEDConfig((byte)StatusLed);
                    device.SetAirPressure(AirPressure);
                    device.SetTemperatureOffset(TemperatureOffset);
                    device.SetAllValuesCallbackConfiguration(Period, false);

                    return Observable.FromEvent<BrickletCO2V2.AllValuesEventHandler, CO2V2DataFrame>(
                        onNext => (sender, co2Concentration, temperature, humidity) =>
                        {
                            onNext(new CO2V2DataFrame(co2Concentration, temperature, humidity));
                        },
                        handler => device.AllValuesCallback += handler,
                        handler => device.AllValuesCallback -= handler)
                        .Finally(() =>
                        {
                            try { device.SetAllValuesCallbackConfiguration(0, false); }
                            catch (NotConnectedException) { } // best effort
                        });
                });
        }
    }

    /// <summary>
    /// Represents a set of measurement values sampled from an CO2 Bricklet 2.0.
    /// </summary>
    public struct CO2V2DataFrame
    {
        /// <summary>
        /// Represents the CO2 concentration in units of 1ppm.
        /// </summary>
        public int CO2Concentration;

        /// <summary>
        /// Represents the current temperature, in hundredths of a degree (celsius).
        /// </summary>
        public short Temperature;

        /// <summary>
        /// Represents the relative humidity in hundredths of percentage points (%RH).
        /// </summary>
        public int Humidity;

        /// <summary>
        /// Initializes a new instance of the <see cref="CO2V2DataFrame"/> structure.
        /// </summary>
        /// <param name="co2Concentration">The CO2 concentration.</param>
        /// <param name="temperature">The current temperature value.</param>
        /// <param name="humidity">The current humidity value.</param>
        public CO2V2DataFrame(int co2Concentration, short temperature, int humidity)
        {
            CO2Concentration = co2Concentration;
            Temperature = temperature;
            Humidity = humidity;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(CO2V2DataFrame)} {{ " +
                $"{nameof(CO2Concentration)} = {CO2Concentration}, " +
                $"{nameof(Temperature)} = {Temperature}, " +
                $"{nameof(Humidity)} = {Humidity} }}";
        }
    }

    /// <summary>
    /// Specifies the behavior of the CO2 Bricklet 2.0. status LED.
    /// </summary>
    public enum CO2V2StatusLedConfig : byte
    {
        /// <summary>
        /// The status LED will be permanently OFF.
        /// </summary>
        Off = BrickletCO2V2.STATUS_LED_CONFIG_OFF,

        /// <summary>
        /// The status LED will be permanently ON as long as the bricklet is powered.
        /// </summary>
        On = BrickletCO2V2.STATUS_LED_CONFIG_ON,

        /// <summary>
        /// The status LED will change state periodically every second.
        /// </summary>
        ShowHeartbeat = BrickletCO2V2.STATUS_LED_CONFIG_SHOW_HEARTBEAT,

        /// <summary>
        /// The LED will show communication traffic between Brick and Bricklet,
        /// flickering once for every 10 received data packets.
        /// </summary>
        ShowStatus = BrickletCO2V2.STATUS_LED_CONFIG_SHOW_STATUS
    }
}
