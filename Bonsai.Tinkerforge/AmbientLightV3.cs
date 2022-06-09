using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    [DefaultProperty(nameof(Uid))]
    [Description("Measures ambient illuminance from an Ambient Light Bricklet 3.0.")]
    public class AmbientLightV3 : Combinator<IPConnection, long>
    {
        [TypeConverter(typeof(UidConverter))]
        [Description("Device data including address UID.")]
        public string Uid { get; set; }

        [Description("Specifies the period between sample event callbacks. A value of zero disables event reporting.")]
        public long Period { get; set; } = 1000;

        [Description("Specifies the measurement range for the ambient light sensor.")]
        public IlluminanceRangeConfig IlluminanceRange { get; set; }

        [Description("Specifies the integration time window used for ambient light measurements.")]
        public IntegrationTimeConfig IntegrationTime { get; set; }

        [Description("Specifies the behavior of the status LED.")]
        public BrickletAmbientLightV3StatusLedConfig StatusLed { get; set; } = BrickletAmbientLightV3StatusLedConfig.ShowStatus;

        public override string ToString()
        {
            return BrickletAmbientLightV3.DEVICE_DISPLAY_NAME;
        }

        public override IObservable<long> Process(IObservable<IPConnection> source)
        {
            return source.SelectStream(connection =>
            {
                var device = new BrickletAmbientLightV3(Uid, connection);
                connection.Connected += (sender, e) =>
                {
                    device.SetStatusLEDConfig((byte)StatusLed);
                    device.SetConfiguration((byte)IlluminanceRange, (byte)IntegrationTime);
                    device.SetIlluminanceCallbackConfiguration(Period, false, 'x', 0, 1);
                };

                return Observable.FromEventPattern<BrickletAmbientLightV3.IlluminanceEventHandler, long>(
                    handler => device.IlluminanceCallback += handler,
                    handler => device.IlluminanceCallback -= handler)
                    .Select(evt => evt.EventArgs)
                    .Finally(() =>
                    {
                        try { device.SetIlluminanceCallbackConfiguration(0, false, 'x', 0, 1); }
                        catch (NotConnectedException) { } // best effort
                    });
            });
        }

        public enum IlluminanceRangeConfig : byte
        {
            Range64000Lux = BrickletAmbientLightV3.ILLUMINANCE_RANGE_64000LUX,
            Range32000Lux = BrickletAmbientLightV3.ILLUMINANCE_RANGE_32000LUX,
            Range16000Lux = BrickletAmbientLightV3.ILLUMINANCE_RANGE_16000LUX,
            Range8000Lux = BrickletAmbientLightV3.ILLUMINANCE_RANGE_8000LUX,
            Range1300Lux = BrickletAmbientLightV3.ILLUMINANCE_RANGE_1300LUX,
            Range600Lux = BrickletAmbientLightV3.ILLUMINANCE_RANGE_600LUX,
            RangeUnlimited = BrickletAmbientLightV3.ILLUMINANCE_RANGE_UNLIMITED
        }

        public enum IntegrationTimeConfig : byte
        {
            Integration50ms = BrickletAmbientLightV3.INTEGRATION_TIME_50MS,
            Integration100ms = BrickletAmbientLightV3.INTEGRATION_TIME_100MS,
            Integration150ms = BrickletAmbientLightV3.INTEGRATION_TIME_150MS,
            Integration200ms = BrickletAmbientLightV3.INTEGRATION_TIME_200MS,
            Integration250ms = BrickletAmbientLightV3.INTEGRATION_TIME_250MS,
            Integration300ms = BrickletAmbientLightV3.INTEGRATION_TIME_300MS,
            Integration350ms = BrickletAmbientLightV3.INTEGRATION_TIME_350MS,
            Integration400ms = BrickletAmbientLightV3.INTEGRATION_TIME_400MS,
        }

        public enum BrickletAmbientLightV3StatusLedConfig : byte
        {
            Off = BrickletAmbientLightV3.STATUS_LED_CONFIG_OFF,
            On = BrickletAmbientLightV3.STATUS_LED_CONFIG_ON,
            ShowHeartbeat = BrickletAmbientLightV3.STATUS_LED_CONFIG_SHOW_HEARTBEAT,
            ShowStatus = BrickletAmbientLightV3.STATUS_LED_CONFIG_SHOW_STATUS
        }
    }
}
