using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    public class GPSV2DateTime : Combinator<IPConnection, GPSV2DateTime.DataFrame>
    {
        [TypeConverter(typeof(UidConverter))]
        [Description("The unique bricklet device UID.")]
        public string Uid { get; set; }

        [Description("Specifies the period between sample event callbacks. A value of zero disables event reporting.")]
        public long Period { get; set; } = 1000;

        [Description("If SBAS (Satellite-based Augmentation System) is enabled, the position accuracy increases (if SBAS satellites are in view), but the update rate is limited to 5Hz. With SBAS disabled the update rate is increased to 10Hz.")]
        public SBASConfig SBAS { get; set; } = SBASConfig.Disabled;

        [Description("Specifies the behavior of the status LED.")]
        public GPSV2SDateTimeStatusLedConfig StatusLed { get; set; } = GPSV2SDateTimeStatusLedConfig.ShowStatus;

        public object deviceLock = new object();

        public override IObservable<DataFrame> Process(IObservable<IPConnection> source)
        {
            return source.SelectStream(connection =>
            {
                var device = new BrickletGPSV2(Uid, connection);
                connection.Connected += (sender, e) =>
                {
                    device.SetStatusLEDConfig((byte)StatusLed); 
                    device.SetSBASConfig((byte)SBAS);
                    device.SetDateTimeCallbackPeriod(Period);
                };

                return Observable.Create<DataFrame>(observer =>
                {
                    BrickletGPSV2.DateTimeEventHandler handler = (sender, date, time) =>
                    {
                        observer.OnNext(new DataFrame(date, time));
                    };

                    device.DateTimeCallback += handler;
                    return Disposable.Create(() =>
                    {
                        try { device.SetDateTimeCallbackPeriod(0); }
                        catch (NotConnectedException) { }
                        device.DateTimeCallback -= handler;
                    });
                });
            });
        }

        public struct DataFrame
        {
            public long Date;
            public long Time;

            public DataFrame(long date, long time)
            {
                Date = date;
                Time = time;
            }
        }

        public enum SBASConfig : byte
        {
            Enabled = BrickletGPSV2.SBAS_ENABLED,
            Disabled = BrickletGPSV2.SBAS_DISABLED,
        }

        public enum GPSV2SDateTimeStatusLedConfig : byte
        {
            Off = BrickletGPSV2.STATUS_LED_CONFIG_OFF,
            On = BrickletGPSV2.STATUS_LED_CONFIG_ON,
            ShowHeartbeat = BrickletGPSV2.STATUS_LED_CONFIG_SHOW_HEARTBEAT,
            ShowStatus = BrickletGPSV2.STATUS_LED_CONFIG_SHOW_STATUS
        }
    }
}
