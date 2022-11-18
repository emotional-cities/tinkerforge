using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    static class ObservableExtensions
    {
        public static IObservable<TResult> SelectStream<TSource, TResult>(this IObservable<TSource> source, Func<TSource, IObservable<TResult>> selector)
        {
            return Observable.Create<TResult>(observer =>
            {
                var sourceDisposable = new CompositeDisposable();
                var connectionObserver = Observer.Create<TSource>(
                    connection =>
                    {
                        var stream = selector(connection);
                        sourceDisposable.Add(stream.SubscribeSafe(observer));
                    });
                return new CompositeDisposable(
                    sourceDisposable,
                    source.SubscribeSafe(connectionObserver));
            });
        }

        public static IObservable<TResult> SelectStream<TDevice, TResult>(
            this IObservable<IPConnection> source,
            Func<IPConnection, TDevice> deviceFactory,
            Func<TDevice, IObservable<TResult>> selector)
        {
            return source.SelectStream(connection =>
            {
                var device = deviceFactory(connection);
                return Observable.FromEventPattern<IPConnection.ConnectedEventHandler, short>(
                    handler => connection.Connected += handler,
                    handler => connection.Connected -= handler)
                    .SelectMany(connected => selector(device));
            });
        }
    }
}
