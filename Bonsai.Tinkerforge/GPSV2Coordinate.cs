using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    [Description("Generates a coordinate data stream from a GPSV2 device.")]
    public class GPSV2Coordinate : Combinator<BrickletGPSV2, GPSV2Coordinate.CoordinateData>
    {
        public override IObservable<CoordinateData> Process(IObservable<BrickletGPSV2> source)
        {
            return source.SelectStream(device =>
            {
                return Observable.Create<CoordinateData>(observer =>
                {
                    BrickletGPSV2.CoordinatesEventHandler handler = (sender, latitude, ns, longitude, ew) =>
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
    }
}
