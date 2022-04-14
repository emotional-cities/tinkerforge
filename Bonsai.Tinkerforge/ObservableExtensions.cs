using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    static class ObservableExtensions
    {
        public static IObservable<TResult> SelectStream<TResult>(this IObservable<IPConnection> source, Func<IPConnection, IObservable<TResult>> selector)
        {
            return Observable.Create<TResult>(observer =>
            {
                var sourceDisposable = new CompositeDisposable();
                var connectionObserver = Observer.Create<IPConnection>(
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
    }
}
