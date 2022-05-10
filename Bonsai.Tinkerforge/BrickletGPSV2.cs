using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Tinkerforge;


namespace Bonsai.Tinkerforge
{
    public class BrickletGPSV2 : Combinator<IPConnection, BrickletGPSV2.CoordinateData>
    {
        [Description("The unique bricklet device UID.")]
        public string Uid { get; set; }

        [Description("Specifies the period between sample event callbacks. A value of zero disables event reporting.")]
        public long Period { get; set; } = 1000;

        [Description("If SBAS is enabled, the position accuracy increases (if SBAS satellites are in view), but the update rate is limited to 5Hz. With SBAS disabled the update rate is increased to 10Hz.")]
        public SBASConfig SBAS { get; set; } = SBASConfig.Disabled;

        [Description("Specifies the behavior of the status LED.")]
        public BrickletGPSV2StatusLedConfig StatusLed { get; set; } = BrickletGPSV2StatusLedConfig.ShowStatus;

        // TODO - can we get all the GPS data structures output here? GetStatus, GetAltitude, GetMotion, GetDateTime, GetSatelliteSystemStatus
        public override IObservable<CoordinateData> Process(IObservable<IPConnection> source)
        {
            return source.SelectStream(connection =>
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
