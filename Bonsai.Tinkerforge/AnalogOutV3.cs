using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using Tinkerforge;
using System.Threading.Tasks;

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

        [Description("Specifies the period between sample event callbacks. A value of zero disables event reporting.")]
        public long Period { get; set; } = 1000;

        [Description("Specifies the behavior of the status LED.")]
        public BrickletAnalogOutV3LedConfig StatusLed { get; set; } = BrickletAnalogOutV3LedConfig.ShowStatus;

        public override string ToString()
        {
            return BrickletAnalogOutV3.DEVICE_DISPLAY_NAME;
        }

        // TO try, make 2 sources and merge?

        //public IObservable<int> Process(IObservable<IPConnection> source, IObservable<int> signal)
        //{
        //    return source.SelectStream(connection =>
        //    {
        //        var device = new BrickletAnalogOutV3(Uid, connection);
        //        connection.Connected += (sender, e) =>
        //        {
        //            device.SetStatusLEDConfig((byte)StatusLed);
        //        };

        //        return Observable.Create<int>(observer =>
        //        {
        //            observer.OnNext(device);
        //        });
        //    });

        //    //Observable.Using()

        //    //return Observable.Using(
        //    //    source.Select(connection =>
        //    //    {
        //    //        var device = new BrickletAnalogOutV3(Uid, connection);
        //    //        connection.Connected += (sender, e) =>
        //    //        {
        //    //            device.SetStatusLEDConfig((byte)StatusLed);
        //    //        };
        //    //        return Observable.Create<BrickletAnalogOutV3>(observer =>
        //    //        {
        //    //            observer.OnNext(device);
        //    //            return Disposable.Create(() =>
        //    //            {
        //    //                try { device.SetStatusLEDConfig(0); }
        //    //                catch (NotConnectedException) { }
        //    //            });
        //    //        });
        //    //    }),
        //    //    dev =>
        //    //    {
        //    //        signal.Do(val => { });
        //    //    }
        //    //);

        //    //return source.SelectMany(connection =>
        //    //{
        //    //    var device = new BrickletAnalogOutV3(Uid, connection);
        //    //    connection.Connected += (sender, e) =>
        //    //    {

        //    //    };
        //    //});
        //}

        //public override IObservable<BrickletAnalogOutV3> Process(IObservable<IPConnection> source)
        //{
        //    return source.SelectStream(connection =>
        //    {
        //        var device = new BrickletAnalogOutV3(Uid, connection);
        //        connection.Connected += (sender, e) =>
        //        {
        //            device.SetStatusLEDConfig((byte)StatusLed);
        //        };

        //        return Observable.Create<BrickletAnalogOutV3>(observer =>
        //        {
        //            observer.OnNext(device);
        //            return Disposable.Create(() =>
        //            {
        //                try { device.SetStatusLEDConfig(0); }
        //                catch (NotConnectedException) { }
        //            });
        //        });
        //    });
        //}

        public enum BrickletAnalogOutV3LedConfig : byte
        {
            Off = BrickletHumidityV2.STATUS_LED_CONFIG_OFF,
            On = BrickletHumidityV2.STATUS_LED_CONFIG_ON,
            ShowHeartbeat = BrickletHumidityV2.STATUS_LED_CONFIG_SHOW_HEARTBEAT,
            ShowStatus = BrickletHumidityV2.STATUS_LED_CONFIG_SHOW_STATUS
        }
    }
}
