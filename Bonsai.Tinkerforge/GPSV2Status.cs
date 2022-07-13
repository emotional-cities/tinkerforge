using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    /// <summary>
    /// Represents an operator that generates a status data stream from a GPS Bricklet 2.0.
    /// </summary>
    [Description("Generates a status data stream from a GPSV2 device.")]
    public class GPSV2Status : Combinator<BrickletGPSV2, GPSV2StatusDataFrame>
    {
        /// <summary>
        /// Measures status data from a GPS Bricklet 2.0.
        /// </summary>
        /// <param name="source">
        /// A sequence containing the connection to a GPS Bricklet 2.0.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="GPSV2StatusDataFrame"/> objects representing the
        /// status measurements from the GPS Bricklet 2.0.
        /// </returns>
        public override IObservable<GPSV2StatusDataFrame> Process(IObservable<BrickletGPSV2> source)
        {
            return source.SelectStream(device =>
            {
                return Observable.Create<GPSV2StatusDataFrame>(observer =>
                {
                    BrickletGPSV2.StatusEventHandler handler = (sender, hasFix, satelliteView) =>
                    {
                        observer.OnNext(new GPSV2StatusDataFrame(hasFix, satelliteView));
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
    }

    /// <summary>
    /// Represents a set of status values sampled from a GPS Bricklet 2.0.
    /// </summary>
    public struct GPSV2StatusDataFrame
    {
        /// <summary>
        /// Represents whether the GPS Bricklet 2.0. has a satellite fix.
        /// </summary>
        public bool HasFix;

        /// <summary>
        /// Represents the number of satellites in view.
        /// </summary>
        public byte SatellitesView;

        /// <summary>
        /// Initializes a new instance of the <see cref="GPSV2StatusDataFrame"/> structure.
        /// </summary>
        /// <param name="hasFix">Whether a fix is available.</param>
        /// <param name="satellitesView">The number of satellites in view.</param>
        public GPSV2StatusDataFrame(bool hasFix, byte satellitesView)
        {
            HasFix = hasFix;
            SatellitesView = satellitesView;
        }
    }
}
