using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;
using Microsoft.Spatial;

namespace EmotionalCities.Tinkerforge
{
    /// <summary>
    /// Represents an operator that retrieves a stream of geography points,
    /// with or without altitude, from a sequence of GPSV2 coordinates.
    /// </summary>
    [Combinator]
    [Description("Retrieves a stream of geography points, with or without altitude, from a sequence of GPSV2 coordinates.")]
    [WorkflowElementCategory(ElementCategory.Transform)]
    public class GPSV2GeographyPoint
    {
        static GeographyPoint CreateGeographyPoint(ref GPSV2CoordinateDataFrame coordinate, double? altitude)
        {
            const double ValueToDegrees = 1.0 / 1000000;
            var latitude = coordinate.Latitude * ValueToDegrees;
            var longitude = coordinate.Longitude * ValueToDegrees;
            if (coordinate.NS == 'S') latitude = -latitude;
            if (coordinate.EW == 'W') longitude = -longitude;
            return GeographyPoint.Create(latitude, longitude, altitude);
        }

        /// <summary>
        /// Retrieves a stream of geography points from a sequence of GPSV2 coordinates.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="GPSV2CoordinateDataFrame"/> representing a GPS
        /// coordinate.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="GeographyPoint"/> objects representing the GPS
        /// location.
        /// </returns>
        public IObservable<GeographyPoint> Process(IObservable<GPSV2CoordinateDataFrame> source)
        {
            return source.Select(input => CreateGeographyPoint(ref input, altitude: null));
        }

        /// <summary>
        /// Retrieves a stream of geography points with altitude information from a
        /// sequence of pairs containing GPSV2 coordinates and altitude data.
        /// </summary>
        /// <param name="source">
        /// A sequence of pairs containing a <see cref="GPSV2CoordinateDataFrame"/>
        /// representing a GPS coordinate, and a <see cref="GPSV2AltitudeDataFrame"/>
        /// representing altitude data.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="GeographyPoint"/> objects representing the GPS
        /// location with altitude information.
        /// </returns>
        public IObservable<GeographyPoint> Process(IObservable<Tuple<GPSV2CoordinateDataFrame, GPSV2AltitudeDataFrame>> source)
        {
            return source.Select(input =>
            {
                const double ValueToAltitude = 1.0 / 100;
                var coordinate = input.Item1;
                var altitude = input.Item2.Altitude * ValueToAltitude;
                return CreateGeographyPoint(ref coordinate, altitude);
            });
        }
    }
}
