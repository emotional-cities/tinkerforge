using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    [DefaultProperty(nameof(Uid))]
    [Description("Generates a device instance for GPS Bricklet 2.0. Can be connected to Coordinate, Altitude, DateTime, Status output streams.")]
    public class GPSV2 : Combinator<IPConnection, BrickletGPSV2>
    {
        [TypeConverter(typeof(UidConverter))]
        [Description("The unique bricklet device UID.")]
        public string Uid { get; set; }

        [Description("If SBAS (Satellite-based Augmentation System) is enabled, the position accuracy increases (if SBAS satellites are in view), but the update rate is limited to 5Hz. With SBAS disabled the update rate is increased to 10Hz.")]
        public SBASConfig SBAS { get; set; } = SBASConfig.Disabled;

        [Description("Specifies the behavior of the status LED.")]
        public BrickletGPSV2StatusLedConfig StatusLed { get; set; } = BrickletGPSV2StatusLedConfig.ShowStatus;

        public override string ToString()
        {
            return BrickletGPSV2.DEVICE_DISPLAY_NAME;
        }

        public override IObservable<BrickletGPSV2> Process(IObservable<IPConnection> source)
        {
            return source.SelectStream(connection =>
            {
                var device = new BrickletGPSV2(Uid, connection);
                connection.Connected += (sender, e) =>
                {
                    device.SetStatusLEDConfig((byte)StatusLed);
                    device.SetSBASConfig((byte)SBAS);
                };

                return Observable.Create<BrickletGPSV2>(observer =>
                {
                    observer.OnNext(device);
                    return Disposable.Create(() =>
                    {
                        try { device.SetStatusLEDConfig(0); }
                        catch (NotConnectedException) { }
                    });
                });
            });
        }

        public enum SBASConfig : byte
        {
            Enabled = BrickletGPSV2.SBAS_ENABLED,
            Disabled = BrickletGPSV2.SBAS_DISABLED,
        }

        public enum BrickletGPSV2StatusLedConfig : byte
        {
            Off = BrickletGPSV2.STATUS_LED_CONFIG_OFF,
            On = BrickletGPSV2.STATUS_LED_CONFIG_ON,
            ShowHeartbeat = BrickletGPSV2.STATUS_LED_CONFIG_SHOW_HEARTBEAT,
            ShowStatus = BrickletGPSV2.STATUS_LED_CONFIG_SHOW_STATUS
        }
    }
}
