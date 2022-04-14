﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    [DefaultProperty(nameof(Uid))]
    [Description("Measures ambient illuminance from an Ambient Light Bricklet 3.0.")]
    public class BrickletAmbientLightV3 : Combinator<IPConnection, long>
    {
        [Description("The unique bricklet device UID.")]
        public string Uid { get; set; }

        [Description("Specifies the period between sample event callbacks. A value of zero disables event reporting.")]
        public long Period { get; set; } = 1000;

        [Description("Specifies the measurement range for the ambient light sensor.")]
        public IlluminanceRangeConfig IlluminanceRange { get; set; }

        [Description("Specifies the integration time window used for ambient light measurements.")]
        public IntegrationTimeConfig IntegrationTime { get; set; }

        [Description("Specifies the behavior of the status LED.")]
        public StatusLedConfig StatusLed { get; set; } = StatusLedConfig.ShowStatus;

        public override IObservable<long> Process(IObservable<IPConnection> source)
        {
            return source.SelectStream(connection =>
            {
                var ambientLight = new global::Tinkerforge.BrickletAmbientLightV3(Uid, connection);
                connection.Connected += (sender, e) =>
                {
                    ambientLight.SetStatusLEDConfig((byte)StatusLed);
                    ambientLight.SetConfiguration((byte)IlluminanceRange, (byte)IntegrationTime);
                    ambientLight.SetIlluminanceCallbackConfiguration(Period, false, 'x', 0, 1);
                };

                return Observable.FromEventPattern<global::Tinkerforge.BrickletAmbientLightV3.IlluminanceEventHandler, long>(
                    handler => ambientLight.IlluminanceCallback += handler,
                    handler => ambientLight.IlluminanceCallback -= handler)
                    .Select(evt => evt.EventArgs)
                    .Finally(() =>
                    {
                        try { ambientLight.SetIlluminanceCallbackConfiguration(0, false, 'x', 0, 1); }
                        catch (NotConnectedException) { } // best effort
                    });
            });
        }

        public enum IlluminanceRangeConfig : byte
        {
            Range64000Lux = global::Tinkerforge.BrickletAmbientLightV3.ILLUMINANCE_RANGE_64000LUX,
            Range32000Lux = global::Tinkerforge.BrickletAmbientLightV3.ILLUMINANCE_RANGE_32000LUX,
            Range16000Lux = global::Tinkerforge.BrickletAmbientLightV3.ILLUMINANCE_RANGE_16000LUX,
            Range8000Lux = global::Tinkerforge.BrickletAmbientLightV3.ILLUMINANCE_RANGE_8000LUX,
            Range1300Lux = global::Tinkerforge.BrickletAmbientLightV3.ILLUMINANCE_RANGE_1300LUX,
            Range600Lux = global::Tinkerforge.BrickletAmbientLightV3.ILLUMINANCE_RANGE_600LUX,
            RangeUnlimited = global::Tinkerforge.BrickletAmbientLightV3.ILLUMINANCE_RANGE_UNLIMITED
        }

        public enum IntegrationTimeConfig : byte
        {
            Integration50ms = global::Tinkerforge.BrickletAmbientLightV3.INTEGRATION_TIME_50MS,
            Integration100ms = global::Tinkerforge.BrickletAmbientLightV3.INTEGRATION_TIME_100MS,
            Integration150ms = global::Tinkerforge.BrickletAmbientLightV3.INTEGRATION_TIME_150MS,
            Integration200ms = global::Tinkerforge.BrickletAmbientLightV3.INTEGRATION_TIME_200MS,
            Integration250ms = global::Tinkerforge.BrickletAmbientLightV3.INTEGRATION_TIME_250MS,
            Integration300ms = global::Tinkerforge.BrickletAmbientLightV3.INTEGRATION_TIME_300MS,
            Integration350ms = global::Tinkerforge.BrickletAmbientLightV3.INTEGRATION_TIME_350MS,
            Integration400ms = global::Tinkerforge.BrickletAmbientLightV3.INTEGRATION_TIME_400MS,
        }

        public enum StatusLedConfig : byte
        {
            Off = global::Tinkerforge.BrickletAmbientLightV3.STATUS_LED_CONFIG_OFF,
            On = global::Tinkerforge.BrickletAmbientLightV3.STATUS_LED_CONFIG_ON,
            ShowHeartbeat = global::Tinkerforge.BrickletAmbientLightV3.STATUS_LED_CONFIG_SHOW_HEARTBEAT,
            ShowStatus = global::Tinkerforge.BrickletAmbientLightV3.STATUS_LED_CONFIG_SHOW_STATUS
        }
    }
}
