using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    [DefaultProperty(nameof(Period))]
    [Description("Generates a status data stream from a GPSV2 device.")]
    public class GPSV2Status : Combinator<BrickletGPSV2, GPSV2Status.StatusData>
    {
        [Description("Specifies the period between sample event callbacks. A value of zero disables event reporting.")]
        public long Period { get; set; } = 1000;

        public override IObservable<StatusData> Process(IObservable<BrickletGPSV2> source)
        {
            return source.SelectStream(device =>
            {
                device.SetStatusCallbackPeriod(Period);
                return Observable.Create<StatusData>(observer =>
                {
                    BrickletGPSV2.StatusEventHandler handler = (sender, hasFix, satelliteView) =>
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
    }
}
