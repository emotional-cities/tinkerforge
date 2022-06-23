using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    [Combinator(MethodName = nameof(Generate))]
    [WorkflowElementCategory(ElementCategory.Source)]
    [Description("Creates a TCP/IP connection to the Brick Daemon.")]
    public class CreateBrickConnection
    {
        internal const string DefaultHostName = "localhost";
        internal const int DefaultPort = 4223;

        [Description("The DNS name of the Brick Daemon on which you intend to connect.")]
        public string HostName { get; set; } = DefaultHostName;

        [Description("The port number of the Brick Daemon on which you intend to connect.")]
        public int Port { get; set; } = DefaultPort;

        [Description("The optional secret used when establishing authenticated connections to Brick Daemon.")]
        public string Secret { get; set; }

        public IObservable<IPConnection> Generate()
        {
            return Observable.Create<IPConnection>(observer =>
            {
                var connection = new IPConnection();
                observer.OnNext(connection);

                var secret = Secret;
                connection.Connect(HostName, Port);
                if (!string.IsNullOrEmpty(secret))
                {
                    connection.Authenticate(secret);
                }

                return Disposable.Create(() => connection.Disconnect());
            });
        }
    }
}
