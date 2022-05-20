using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    [DefaultProperty(nameof(Uid))]
    [Description("Measures coordinates, altitude, modtion, timestamp, satellite status from a GPS Bricklet 2.0.")]
    public class BrickletGPSV2 : Combinator<IPConnection, Tuple<BrickletGPSV2.StatusData, BrickletGPSV2.CoordinateData, BrickletGPSV2.AltitudeData, BrickletGPSV2.DateTimeData>>
    {
        [Description("The unique bricklet device UID.")]
        public string Uid { get; set; }

        [Description("Specifies the period between sample event callbacks. A value of zero disables event reporting.")]
        public long Period { get; set; } = 1000;

        [Description("If SBAS is enabled, the position accuracy increases (if SBAS satellites are in view), but the update rate is limited to 5Hz. With SBAS disabled the update rate is increased to 10Hz.")]
        public SBASConfig SBAS { get; set; } = SBASConfig.Disabled;

        [Description("Specifies the behavior of the status LED.")]
        public BrickletGPSV2StatusLedConfig StatusLed { get; set; } = BrickletGPSV2StatusLedConfig.ShowStatus;

        // TODO - can we get all the GPS data structures output here? GetStatus, GetAltitude, GetMotion, GetDateTime, GetSatelliteSystemStatus - could generate several observables for each Get function and then Merge
        // TODO - there is alot of repeated code here. gps-struct-test branch has a potential solution but is arguably worse to read. Hard to create generic StreamFactory method due to multiple separate event handlers and data types on each Bricklet 
        public override IObservable<Tuple<StatusData, CoordinateData, AltitudeData, DateTimeData>> Process(IObservable<IPConnection> source)
        {
            // Status stream
            IObservable<StatusData> statusStream = source.SelectStream(connection =>
            {
                var device = new global::Tinkerforge.BrickletGPSV2(Uid, connection);
                connection.Connected += (sender, e) =>
                {
                    device.SetStatusCallbackPeriod(Period);
                };

                return Observable.Create<StatusData>(observer =>
                {
                    observer.OnNext(new StatusData()); // Initialize an empty data struct in case one of the data providers doesn't return anything to the stream combination

                    global::Tinkerforge.BrickletGPSV2.StatusEventHandler handler = (sender, hasFix, satelliteView) =>
                    {
                        observer.OnNext(new StatusData(hasFix, satelliteView));
                    };

                    device.StatusCallback += handler;
                    return Disposable.Create(() =>
                    {
                        try { device.SetStatusCallbackPeriod(0); }
                        catch (NotConnectedException) { }
                        device.StatusCallback -= handler;
                    });
                });
            });

            // Altitude stream
            IObservable<AltitudeData> altitudeStream = source.SelectStream(connection =>
            {
                var device = new global::Tinkerforge.BrickletGPSV2(Uid, connection);
                connection.Connected += (sender, e) =>
                {
                    device.SetAltitudeCallbackPeriod(Period);
                };

                return Observable.Create<AltitudeData>(observer =>
                {
                    observer.OnNext(new AltitudeData()); // Initialize an empty data struct in case one of the data providers doesn't return anything to the stream combination

                    global::Tinkerforge.BrickletGPSV2.AltitudeEventHandler handler = (sender, altitude, geoidalSeparation) =>
                    {
                        observer.OnNext(new AltitudeData(altitude, geoidalSeparation));
                    };

                    device.AltitudeCallback += handler;
                    return Disposable.Create(() =>
                    {
                        try { device.SetAltitudeCallbackPeriod(0); }
                        catch (NotConnectedException) { }
                        device.AltitudeCallback -= handler;
                    });
                });
            });

            // DateTime stream
            IObservable<DateTimeData> dateTimeStream = source.SelectStream(connection =>
            {
                var device = new global::Tinkerforge.BrickletGPSV2(Uid, connection);
                connection.Connected += (sender, e) =>
                {
                    device.SetDateTimeCallbackPeriod(Period);
                };

                return Observable.Create<DateTimeData>(observer =>
                {
                    observer.OnNext(new DateTimeData()); // Initialize an empty data struct in case one of the data providers doesn't return anything to the stream combination

                    global::Tinkerforge.BrickletGPSV2.DateTimeEventHandler handler = (sender, date, time) =>
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

            // Coordinate stream
            IObservable<CoordinateData> coordinateStream = source.SelectStream(connection =>
            {
                var device = new global::Tinkerforge.BrickletGPSV2(Uid, connection);
                connection.Connected += (sender, e) =>
                {
                    device.SetStatusLEDConfig((byte)StatusLed);
                    device.SetSBASConfig((byte)SBAS);
                    device.SetCoordinatesCallbackPeriod(Period);
                };

                return Observable.Create<CoordinateData>(observer =>
                {
                    observer.OnNext(new CoordinateData()); // Initialize an empty data struct in case one of the data providers doesn't return anything to the stream combination

                    global::Tinkerforge.BrickletGPSV2.CoordinatesEventHandler handler = (sender, latitude, ns, longitude, ew) =>
                    {
                        observer.OnNext(new CoordinateData(latitude, longitude, ns, ew));
                    };

                    device.CoordinatesCallback += handler;
                    return Disposable.Create(() =>
                    {
                        try { device.SetCoordinatesCallbackPeriod(0); }
                        catch (NotConnectedException) { }
                        device.CoordinatesCallback -= handler;
                    });
                });
            });

            return statusStream.CombineLatest(coordinateStream, altitudeStream, dateTimeStream, (s1, s2, s3, s4) => Tuple.Create(s1, s2, s3, s4));
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

            public AltitudeData (int altitude, int geoidalSeparation)
            {
                Altitude = altitude;
                GeoidalSeparation = geoidalSeparation;
            }
        }

        public struct DateTimeData
        {
            public long Date;
            public long Time;

            public DateTimeData (long date, long time)
            {
                Date = date;
                Time = time;
            }
        }

        public enum SBASConfig : byte
        {
            Enabled = global::Tinkerforge.BrickletGPSV2.SBAS_ENABLED,
            Disabled = global::Tinkerforge.BrickletGPSV2.SBAS_DISABLED,
        }

        public enum BrickletGPSV2StatusLedConfig : byte
        {
            Off = global::Tinkerforge.BrickletGPSV2.STATUS_LED_CONFIG_OFF,
            On = global::Tinkerforge.BrickletGPSV2.STATUS_LED_CONFIG_ON,
            ShowHeartbeat = global::Tinkerforge.BrickletGPSV2.STATUS_LED_CONFIG_SHOW_HEARTBEAT,
            ShowStatus = global::Tinkerforge.BrickletGPSV2.STATUS_LED_CONFIG_SHOW_STATUS
        }
    }
}
