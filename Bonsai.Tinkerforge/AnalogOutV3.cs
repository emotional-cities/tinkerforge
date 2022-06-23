﻿using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    [Combinator]
    [DefaultProperty(nameof(Uid))]
    [Description("Writes an analog output signal (int x.xxx, 3300 = 3.3V, 0-12V range) to an Analog OUT Bricklet 3.0.")]
    public class AnalogOutV3
    {
        [TypeConverter(typeof(UidConverter))]
        [Description("The unique bricklet device UID.")]
        public string Uid { get; set; }

        [Description("Sets the initial voltage on initialisation. If a negative value is provided the voltage will not be changed on initialisation.")]
        public int InitialVoltage { get; set; } = 0;

        [Description("Specifies the behavior of the status LED.")]
        public BrickletAnalogOutV3LedConfig StatusLed { get; set; } = BrickletAnalogOutV3LedConfig.ShowStatus;

        public override string ToString()
        {
            return BrickletAnalogOutV3.DEVICE_DISPLAY_NAME;
        }

        public IObservable<int> Process(IObservable<int> signal, IObservable<IPConnection> source)
        {
            return source.SelectStream(connection =>
            {
                var device = new BrickletAnalogOutV3(Uid, connection);
                connection.Connected += (sender, e) =>
                {
                    device.SetStatusLEDConfig((byte)StatusLed);
                    if (InitialVoltage >= 0)
                        device.SetOutputVoltage(InitialVoltage);
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
            }).SelectMany(device => signal.Do(x => device.SetOutputVoltage(x)));

            //return deviceStream.CombineLatest(signal, (x, y) => {
            //    try { x.SetOutputVoltage(y); }
            //    catch { }
            //    return y; }
            //);
        }

        public enum BrickletAnalogOutV3LedConfig : byte
        {
            Off = BrickletHumidityV2.STATUS_LED_CONFIG_OFF,
            On = BrickletHumidityV2.STATUS_LED_CONFIG_ON,
            ShowHeartbeat = BrickletHumidityV2.STATUS_LED_CONFIG_SHOW_HEARTBEAT,
            ShowStatus = BrickletHumidityV2.STATUS_LED_CONFIG_SHOW_STATUS
        }
    }
}
