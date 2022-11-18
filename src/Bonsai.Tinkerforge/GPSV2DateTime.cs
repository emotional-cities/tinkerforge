using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    /// <summary>
    /// Represents an operator that generates a datetime data stream from a GPS Bricklet 2.0.
    /// </summary>
    [Description("Generates a datetime data stream from a GPSV2 device.")]
    public class GPSV2DateTime : Combinator<BrickletGPSV2, GPSV2DateTimeDataFrame>
    {
        /// <summary>
        /// Measures datetime data from a GPS Bricklet 2.0.
        /// </summary>
        /// <param name="source">
        /// A sequence containing the connection to a GPS Bricklet 2.0.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="GPSV2DateTimeDataFrame"/> objects representing the
        /// datetime measurements from the GPS Bricklet 2.0.
        /// </returns>
        public override IObservable<GPSV2DateTimeDataFrame> Process(IObservable<BrickletGPSV2> source)
        {
            return source.SelectStream(device =>
            {
                return Observable.FromEvent<BrickletGPSV2.DateTimeEventHandler, GPSV2DateTimeDataFrame>(
                    onNext => (sender, date, time) =>
                    {
                        onNext(new GPSV2DateTimeDataFrame(date, time));
                    },
                    handler => device.DateTimeCallback += handler,
                    handler => device.DateTimeCallback -= handler)
                    .Finally(() =>
                    {
                        try { device.SetDateTimeCallbackPeriod(0); }
                        catch (NotConnectedException) { }
                    });
            });
        }
    }

    /// <summary>
    /// Represents a set of altitude values sampled from a GPS Bricklet 2.0.
    /// </summary>
    public struct GPSV2DateTimeDataFrame
    {
        /// <summary>
        /// Represents the date in ddmmyy format.
        /// </summary>
        public long Date;

        /// <summary>
        /// Represents the time in hhmmss.sss format.
        /// </summary>
        public long Time;

        /// <summary>
        /// Initializes a new instance of the <see cref="GPSV2DateTimeDataFrame"/> structure.
        /// </summary>
        /// <param name="date">The date value (ddmmyy).</param>
        /// <param name="time">The time value (hhmmss.sss).</param>
        public GPSV2DateTimeDataFrame(long date, long time)
        {
            Date = date;
            Time = time;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(GPSV2DateTimeDataFrame)} {{ " +
                $"{nameof(Date)} = {Date}, " +
                $"{nameof(Time)} = {Time} }}";
        }
    }
}
