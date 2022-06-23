using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    [Description("Generates an altitude data stream from a GPSV2 device.")]
    public class GPSV2Altitude : Combinator<BrickletGPSV2, GPSV2Altitude.AltitudeData>
    {
        public override IObservable<AltitudeData> Process(IObservable<BrickletGPSV2> source)
        {
            return source.SelectStream(device =>
            {
                return Observable.Create<AltitudeData>(observer =>
                {
                    BrickletGPSV2.AltitudeEventHandler handler = (sender, altitude, geoidalSeparation) =>
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
    }
}
