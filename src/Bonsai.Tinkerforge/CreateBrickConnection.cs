using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    /// <summary>
    /// Represents an operator that creates a TCP/IP connection to the Brick Daemon.
    /// </summary>
    [Combinator(MethodName = nameof(Generate))]
    [WorkflowElementCategory(ElementCategory.Source)]
    [Description("Creates a TCP/IP connection to the Brick Daemon.")]
    public class CreateBrickConnection
    {
        internal const string DefaultHostName = "localhost";
        internal const int DefaultPort = 4223;

        /// <summary>
        /// Gets or sets a value specifying the DNS name of the Brick Daemon on which you intend to connect.
        /// </summary>
        [Description("The DNS name of the Brick Daemon on which you intend to connect.")]
        public string HostName { get; set; } = DefaultHostName;

        /// <summary>
        /// Gets or sets a value specifying port number of the Brick Daemon on which you intend to connect.
        /// </summary>
        [Description("The port number of the Brick Daemon on which you intend to connect.")]
        public int Port { get; set; } = DefaultPort;

        /// <summary>
        /// Gets or sets a value specifying the optional secred used when establishing authenticated connections to the Brick Daemon
        /// </summary>
        [Description("The optional secret used when establishing authenticated connections to Brick Daemon.")]
        public string Secret { get; set; }

        /// <summary>
        /// Creates a TCP/IP connection to the Brick Daemon
        /// </summary>
        /// <returns>
        /// A sequence of <see cref="IPConnection"/> objects representing the
        /// connection to the Brick Daemon.
        /// </returns>
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
