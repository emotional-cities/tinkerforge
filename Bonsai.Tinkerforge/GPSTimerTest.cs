using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    [DefaultProperty(nameof(Uid))]
    [Description("Debug Timer from a GPS Bricklet 2.0.")]
    public class GPSTimerTest : Combinator<IPConnection, DateTimeData>
    {
        [Description("The unique bricklet device UID.")]
        public string Uid { get; set; }

        [Description("Specifies the period between sample event callbacks. A value of zero disables event reporting.")]
        public long Period { get; set; } = 100;

        [Description("If SBAS is enabled, the position accuracy increases (if SBAS satellites are in view), but the update rate is limited to 5Hz. With SBAS disabled the update rate is increased to 10Hz.")]
        public SBASConfig SBAS { get; set; } = SBASConfig.Disabled;

        [Description("Specifies the behavior of the status LED.")]
        public BrickletGPSV2StatusLedConfig StatusLed { get; set; } = BrickletGPSV2StatusLedConfig.ShowStatus;

        public override IObservable<DateTimeData> Process(IObservable<IPConnection> source)
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

                return Observable.Create<DateTimeData>(observer =>
                {
                    BrickletGPSV2.DateTimeEventHandler handler = (sender, date, time) =>
                    {
                        observer.OnNext(new DateTimeData(date, time));
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
    }

    public struct CoordinateData
    {
        public long Latitude;
        public char NS;
        public long Longitude;
        public char EW;

        public CoordinateData(long latitude, long longitude, char ns, char ew)
        {
            Latitude = latitude;
            Longitude = longitude;
            NS = ns;
            EW = ew;
        }
    }

    public struct StatusData
    {
        public bool HasFix;
        public byte SatellitesView;

        public StatusData(bool hasFix, byte satellitesView)
        {
            HasFix = hasFix;
            SatellitesView = satellitesView;
        }
    }

    public struct AltitudeData
    {
        public int Altitude;
        public int GeoidalSeparation;

        public AltitudeData(int altitude, int geoidalSeparation)
        {
            Altitude = altitude;
            GeoidalSeparation = geoidalSeparation;
        }
    }

    public struct DateTimeData
    {
        public long Date;
        public long Time;

        public DateTimeData(long date, long time)
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

    public enum BrickletGPSV2StatusLedConfig : byte
    {
        Off = BrickletGPSV2.STATUS_LED_CONFIG_OFF,
        On = BrickletGPSV2.STATUS_LED_CONFIG_ON,
        ShowHeartbeat = BrickletGPSV2.STATUS_LED_CONFIG_SHOW_HEARTBEAT,
        ShowStatus = BrickletGPSV2.STATUS_LED_CONFIG_SHOW_STATUS
    }
}
