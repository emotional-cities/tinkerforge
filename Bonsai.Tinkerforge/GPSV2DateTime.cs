using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    [DefaultProperty(nameof(Period))]
    [Description("Generates a datetime data stream from a GPSV2 device.")]
    public class GPSV2DateTime : Combinator<BrickletGPSV2, GPSV2DateTime.DateTimeData>
    {
        [Description("Specifies the period between sample event callbacks. A value of zero disables event reporting.")]
        public long Period { get; set; } = 1000;

        public override IObservable<DateTimeData> Process(IObservable<BrickletGPSV2> source)
        {
            return source.SelectStream(device =>
            {
                device.SetDateTimeCallbackPeriod(Period);
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

        public struct DateTimeData
        {
            public long Date;
            public long Time;

            public DateTimeData(long date, long time)
            {
                Date = date;
                Time = time;
            }
        }
    }
}
