using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    /// <summary>
    /// Represents an operator that generates an altitude data stream from a GPS Bricklet 2.0.
    /// </summary>
    [Description("Generates an altitude data stream from a GPS Bricklet 2.0.")]
    public class GPSV2Altitude : Combinator<BrickletGPSV2, GPSV2AltitudeDataFrame>
    {
        /// <summary>
        /// Measures altitude data from a GPS Bricklet 2.0.
        /// </summary>
        /// <param name="source">
        /// A sequence containing the connection to a GPS Bricklet 2.0.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="GPSV2AltitudeDataFrame"/> objects representing the
        /// altitude measurements from the GPS Bricklet 2.0.
        /// </returns>
        public override IObservable<GPSV2AltitudeDataFrame> Process(IObservable<BrickletGPSV2> source)
        {
            return source.SelectStream(device =>
            {
                return Observable.Create<GPSV2AltitudeDataFrame>(observer =>
                {
                    BrickletGPSV2.AltitudeEventHandler handler = (sender, altitude, geoidalSeparation) =>
                    {
                        observer.OnNext(new GPSV2AltitudeDataFrame(altitude, geoidalSeparation));
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
    }

    /// <summary>
    /// Represents a set of altitude values sampled from a GPS Bricklet 2.0.
    /// </summary>
    public struct GPSV2AltitudeDataFrame
    {
        /// <summary>
        /// Represents the altitude in units of 1cm.
        /// </summary>
        public int Altitude;

        /// <summary>
        /// Represents the geoidal separation in units of 1cm.
        /// </summary>
        public int GeoidalSeparation;

        /// <summary>
        /// Initializes a new instance of the <see cref="GPSV2AltitudeDataFrame"/> structure.
        /// </summary>
        /// <param name="altitude">The altitude value.</param>
        /// <param name="geoidalSeparation">The geoidal separation.</param>
        public GPSV2AltitudeDataFrame(int altitude, int geoidalSeparation)
        {
            Altitude = altitude;
            GeoidalSeparation = geoidalSeparation;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(GPSV2AltitudeDataFrame)} {{ " +
                $"{nameof(Altitude)} = {Altitude}, " +
                $"{nameof(GeoidalSeparation)} = {GeoidalSeparation} }}";
        }
    }
}
