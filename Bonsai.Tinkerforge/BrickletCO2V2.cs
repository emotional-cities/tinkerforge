using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    [Combinator]
    [DefaultProperty(nameof(Uid))]
    [Description("Measures CO2 concentration, in ppm, temperature, and humidity from a CO2 Bricklet 2.0.")]
    public class BrickletCO2V2
    {
        [Description("The unique bricklet device UID.")]
        public string Uid { get; set; }

        [Description("Specifies the period between sample event callbacks. A value of zero disables event reporting.")]
        public long Period { get; set; } = 1000;

        [Description("Specifies the ambient air pressure. Can be used to increase the accuracy of the CO2 sensor.")]
        public int AirPressure { get; set; }

        [Description("Specifies a temperature offset, in hundredths of a degree, to compensate for heat inside an enclosure.")]
        public int TemperatureOffset { get; set; }

        public IObservable<DataFrame> Process(IObservable<IPConnection> source)
        {
            return source.SelectStream(connection =>
            {
                var device = new global::Tinkerforge.BrickletCO2V2(Uid, connection);
                connection.Connected += (sender, e) =>
                {
                    device.SetAirPressure(AirPressure);
                    device.SetTemperatureOffset(TemperatureOffset);
                    device.SetAllValuesCallbackConfiguration(Period, false);
                };

                return Observable.Create<DataFrame>(observer =>
                {
                    global::Tinkerforge.BrickletCO2V2.AllValuesEventHandler handler = (sender, co2Concentration, temperature, humidity) =>
                    {
                        observer.OnNext(new DataFrame(co2Concentration, temperature, humidity));
                    };

                    device.AllValuesCallback += handler;
                    return Disposable.Create(() => device.AllValuesCallback -= handler);
                });
            });
        }

        public struct DataFrame
        {
            public int Co2Concentration;
            public short Temperature;
            public int Humidity;

            public DataFrame(int co2Concentration, short temperature, int humidity)
            {
                Co2Concentration = co2Concentration;
                Temperature = temperature;
                Humidity = humidity;
            }
        }
    }
}
