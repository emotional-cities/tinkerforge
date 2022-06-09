﻿using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    [DefaultProperty(nameof(Uid))]
    [Description("Measures different sizes of particulate matter from a Particulate Matter Bricklet.")]
    public class ParticulateMatter : Combinator<IPConnection, ParticulateMatter.DataFrame>
    {
        [TypeConverter(typeof(UidConverter))]
        [Description("The unique bricklet device UID.")]
        public string Uid { get; set; }

        [Description("Specifies the period between sample event callbacks. A value of zero disables event reporting.")]
        public long Period { get; set; } = 1000;

        [Description("Specifies the behavior of the status LED.")]
        public BrickletParticulateMatterStatusLedConfig StatusLed { get; set; } = BrickletParticulateMatterStatusLedConfig.ShowStatus;

        public override IObservable<DataFrame> Process(IObservable<IPConnection> source)
        {
            return source.SelectStream(connection =>
            {
                var device = new BrickletParticulateMatter(Uid, connection);
                connection.Connected += (sender, e) =>
                {
                    device.SetStatusLEDConfig((byte)StatusLed);
                    device.SetPMConcentrationCallbackConfiguration(Period, false);
                };

                return Observable.Create<DataFrame>(observer =>
                {
                    BrickletParticulateMatter.PMConcentrationEventHandler handler = (sender, pm10, pm25, pm100) =>
                    {
                        observer.OnNext(new DataFrame(pm10, pm25, pm100));
                    };

                    device.PMConcentrationCallback += handler;
                    return Disposable.Create(() =>
                    {
                        try { device.SetPMConcentrationCallbackConfiguration(0, false); }
                        catch (NotConnectedException) { }
                        device.PMConcentrationCallback -= handler;
                    });
                });
            });
        }

        public struct DataFrame
        {
            public int Pm10;
            public int Pm25;
            public int Pm100;

            public DataFrame(int pm10, int pm25, int pm100)
            {
                Pm10 = pm10;
                Pm25 = pm25;
                Pm100 = pm100;
            }
        }

        public enum BrickletParticulateMatterStatusLedConfig : byte
        {
            Off = BrickletParticulateMatter.STATUS_LED_CONFIG_OFF,
            On = BrickletParticulateMatter.STATUS_LED_CONFIG_ON,
            ShowHeartbeat = BrickletParticulateMatter.STATUS_LED_CONFIG_SHOW_HEARTBEAT,
            ShowStatus = BrickletParticulateMatter.STATUS_LED_CONFIG_SHOW_STATUS
        }
    }
}