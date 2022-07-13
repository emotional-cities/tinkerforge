using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    /// <summary>
    /// Represents an operator that generates a coordinate data stream from a GPS Bricklet 2.0.
    /// </summary>
    [Description("Generates a coordinate data stream from a GPSV2 device.")]
    public class GPSV2Coordinate : Combinator<BrickletGPSV2, GPSV2CoordinateDataFrame>
    {
        /// <summary>
        /// Measures coordinate data from a GPS Bricklet 2.0.
        /// </summary>
        /// <param name="source">
        /// A sequence containing the connection to a GPS Bricklet 2.0.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="GPSV2CoordinateDataFrame"/> objects representing the
        /// coordinate measurements from the GPS Bricklet 2.0.
        /// </returns>
        public override IObservable<GPSV2CoordinateDataFrame> Process(IObservable<BrickletGPSV2> source)
        {
            return source.SelectStream(device =>
            {
                return Observable.Create<GPSV2CoordinateDataFrame>(observer =>
                {
                    BrickletGPSV2.CoordinatesEventHandler handler = (sender, latitude, ns, longitude, ew) =>
                    {
                        observer.OnNext(new GPSV2CoordinateDataFrame(latitude, longitude, ns, ew));
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
    }

    /// <summary>
    /// Represents a set of coordinate values sampled from a GPS Bricklet 2.0.
    /// </summary>
    public struct GPSV2CoordinateDataFrame
    {
        /// <summary>
        /// Represents the latitude in units of 1/1000000ths of a degree.
        /// </summary>
        public long Latitude;

        /// <summary>
        /// Represents the cardinal direction of the latitude value.
        /// </summary>
        public char NS;

        /// <summary>
        /// Represents the longitude in units of 1/1000000ths of a degree.
        /// </summary>
        public long Longitude;

        /// <summary>
        /// Represents the cardinal direction of the longitude value.
        /// </summary>
        public char EW;

        /// <summary>
        /// Initializes a new instance of the <see cref="GPSV2CoordinateDataFrame"/> structure.
        /// </summary>
        /// <param name="latitude">The latitude value.</param>
        /// <param name="longitude">The longitude value.</param>
        /// <param name="ns">The cardinal direction of the latitude value ('N' or 'S').</param>
        /// <param name="ew">The cardinal direction of the longitude value ('E' or 'W').</param>
        public GPSV2CoordinateDataFrame(long latitude, long longitude, char ns, char ew)
        {
            Latitude = latitude;
            Longitude = longitude;
            NS = ns;
            EW = ew;
        }
    }
}
