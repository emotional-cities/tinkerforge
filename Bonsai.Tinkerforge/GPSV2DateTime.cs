using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    /// <summary>
    /// Represents an operator that generates a datetime data stream from a GPS Bricklet 2.0.
    /// </summary>
    [Description("Generates a datetime data stream from a GPSV2 device.")]
    public class GPSV2DateTime : Combinator<BrickletGPSV2, GPSV2DateTime.DateTimeData>
    {
        /// <summary>
        /// Measures datetime data from a GPS Bricklet 2.0.
        /// </summary>
        /// <param name="source">
        /// A sequence containing the connection to a GPS Bricklet 2.0.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="DateTimeData"/> objects representing the
        /// datetime measurements from the GPS Bricklet 2.0.
        /// </returns>
        public override IObservable<DateTimeData> Process(IObservable<BrickletGPSV2> source)
        {
            return source.SelectStream(device =>
            {
                return Observable.Create<DateTimeData>(observer =>
                {
                    BrickletGPSV2.DateTimeEventHandler handler = (sender, date, time) =>
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
        }

        /// <summary>
        /// Represents a set of altitude values sampled from a GPS Bricklet 2.0.
        /// </summary>
        public struct DateTimeData
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
            /// Initializes a new instance of the <see cref="DateTimeData"/> structure.
            /// </summary>
            /// <param name="date">The date value (ddmmyy).</param>
            /// <param name="time">The time value (hhmmss.sss).</param>
            public DateTimeData(long date, long time)
            {
                Date = date;
                Time = time;
            }
        }
    }
}
