using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    [DefaultProperty(nameof(Uid))]
    [Description("Generates a device instance for GPS Bricklet 2.0. Can be connected to Coordinate, Altitude, DateTime, Status output streams.")]
    public class GPSV2 : Combinator<IPConnection, BrickletGPSV2>
    {
        [TypeConverter(typeof(UidConverter))]
        [Description("The unique bricklet device UID.")]
        public string Uid { get; set; }

        [Description("If SBAS (Satellite-based Augmentation System) is enabled, the position accuracy increases (if SBAS satellites are in view), but the update rate is limited to 5Hz. With SBAS disabled the update rate is increased to 10Hz.")]
        public SBASConfig SBAS { get; set; } = SBASConfig.Disabled;

        [Description("Specifies the behavior of the status LED.")]
        public BrickletGPSV2StatusLedConfig StatusLed { get; set; } = BrickletGPSV2StatusLedConfig.ShowStatus;

        public override IObservable<BrickletGPSV2> Process(IObservable<IPConnection> source)
        {
            return source.SelectStream(connection =>
            {
                var device = new BrickletGPSV2(Uid, connection);
                connection.Connected += (sender, e) =>
                {
                    device.SetStatusLEDConfig((byte)StatusLed);
                    device.SetSBASConfig((byte)SBAS);
                };

                return Observable.Create<BrickletGPSV2>(observer =>
                {
                    observer.OnNext(device);
                    return Disposable.Create(() =>
                    {
                        try { device.SetStatusLEDConfig(0); }
                        catch (NotConnectedException) { }
                    });
                });
            });
        }

        //public override IObservable<Tuple<CoordinateData, StatusData, AltitudeData, DateTimeData>> Process(IObservable<IPConnection> source)
        //{
        //    // Coordinate stream
        //    IObservable<CoordinateData> coordinateStream = source.SelectStream(connection =>
        //    {
        //        var device = new BrickletGPSV2(Uid, connection);
        //        connection.Connected += (sender, e) =>
        //        {
        //            device.SetStatusLEDConfig((byte)StatusLed);
        //            device.SetSBASConfig((byte)SBAS);
        //            device.SetCoordinatesCallbackPeriod(Period);
        //        };

        //        return Observable.Create<CoordinateData>(observer =>
        //        {
        //            observer.OnNext(new CoordinateData()); // Initialize an empty data struct in case one of the data providers doesn't return anything to the stream combination

        //            BrickletGPSV2.CoordinatesEventHandler handler = (sender, latitude, ns, longitude, ew) =>
        //            {
        //                observer.OnNext(new CoordinateData(latitude, longitude, ns, ew));
        //            };

        //            device.CoordinatesCallback += handler;
        //            return Disposable.Create(() =>
        //            {
        //                try { device.SetCoordinatesCallbackPeriod(0); }
        //                catch (NotConnectedException) { }
        //                device.CoordinatesCallback -= handler;
        //            });
        //        });
        //    });

        //    // Status stream
        //    IObservable<StatusData> statusStream = source.SelectStream(connection =>
        //    {
        //        var device = new BrickletGPSV2(Uid, connection);
        //        connection.Connected += (sender, e) =>
        //        {
        //            device.SetStatusCallbackPeriod(Period);
        //        };

        //        return Observable.Create<StatusData>(observer =>
        //        {
        //            observer.OnNext(new StatusData()); // Initialize an empty data struct in case one of the data providers doesn't return anything to the stream combination

        //            BrickletGPSV2.StatusEventHandler handler = (sender, hasFix, satelliteView) =>
        //            {
        //                observer.OnNext(new StatusData(hasFix, satelliteView));
        //            };

        //            device.StatusCallback += handler;
        //            return Disposable.Create(() =>
        //            {
        //                try { device.SetStatusCallbackPeriod(0); }
        //                catch (NotConnectedException) { }
        //                device.StatusCallback -= handler;
        //            });
        //        });
        //    });

        //    // Altitude stream
        //    IObservable<AltitudeData> altitudeStream = source.SelectStream(connection =>
        //    {
        //        var device = new BrickletGPSV2(Uid, connection);
        //        connection.Connected += (sender, e) =>
        //        {
        //            device.SetAltitudeCallbackPeriod(Period);
        //        };

        //        return Observable.Create<AltitudeData>(observer =>
        //        {
        //            observer.OnNext(new AltitudeData()); // Initialize an empty data struct in case one of the data providers doesn't return anything to the stream combination

        //            BrickletGPSV2.AltitudeEventHandler handler = (sender, altitude, geoidalSeparation) =>
        //            {
        //                observer.OnNext(new AltitudeData(altitude, geoidalSeparation));
        //            };

        //            device.AltitudeCallback += handler;
        //            return Disposable.Create(() =>
        //            {
        //                try { device.SetAltitudeCallbackPeriod(0); }
        //                catch (NotConnectedException) { }
        //                device.AltitudeCallback -= handler;
        //            });
        //        });
        //    });

        //    // DateTime stream
        //    IObservable<DateTimeData> dateTimeStream = source.SelectStream(connection =>
        //    {
        //        var device = new BrickletGPSV2(Uid, connection);
        //        connection.Connected += (sender, e) =>
        //        {
        //            device.SetDateTimeCallbackPeriod(Period);
        //        };

        //        return Observable.Create<DateTimeData>(observer =>
        //        {
        //            observer.OnNext(new DateTimeData()); // Initialize an empty data struct in case one of the data providers doesn't return anything to the stream combination

        //            BrickletGPSV2.DateTimeEventHandler handler = (sender, date, time) =>
        //            {
        //                observer.OnNext(new DateTimeData(date, time));
        //            };

        //            device.DateTimeCallback += handler;
        //            return Disposable.Create(() =>
        //            {
        //                try { device.SetDateTimeCallbackPeriod(0); }
        //                catch (NotConnectedException) { }
        //                device.DateTimeCallback -= handler;
        //            });
        //        });
        //    });

        //    return coordinateStream.CombineLatest(statusStream, altitudeStream, dateTimeStream, (s1, s2, s3, s4) => Tuple.Create(s1, s2, s3, s4));
        ////}

        //public struct StatusData
        //{
        //    public bool HasFix;
        //    public byte SatellitesView;

        //    public StatusData(bool hasFix, byte satellitesView)
        //    {
        //        HasFix = hasFix;
        //        SatellitesView = satellitesView;
        //    }
        //}

        //public struct AltitudeData
        //{
        //    public int Altitude;
        //    public int GeoidalSeparation;

        //    public AltitudeData (int altitude, int geoidalSeparation)
        //    {
        //        Altitude = altitude;
        //        GeoidalSeparation = geoidalSeparation;
        //    }
        //}

        //public struct DateTimeData
        //{
        //    public long Date;
        //    public long Time;

        //    public DateTimeData (long date, long time)
        //    {
        //        Date = date;
        //        Time = time;
        //    }
        //}

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
}
