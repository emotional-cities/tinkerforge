using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    [DefaultProperty(nameof(Uid))]
    [Description("Measures sound pressure and specture from a Sound Pressure Level Bricklet")]
    public class BrickletSoundPressure : Combinator<IPConnection, int>
    {
        [Description("The unique bricklet device UID.")]
        public string Uid { get; set; }

        [Description("Specifies the period between sample event callbacks. A value of zero disables event reporting.")]
        public long Period { get; set; } = 1000;

        [Description("Specifies the size, resolution and bin size of the FFT.")]
        public FftSize FFTSize { get; set; } = FftSize.FftSize512;

        [Description("Specifies the dB weighting function.")]
        public WeightingFunction Weighting { get; set; } = WeightingFunction.WeightingITU;

        [Description("Specifies the behavior of the status LED.")]
        public BrickletSoundPressureStatusLedConfig StatusLed { get; set; } = BrickletSoundPressureStatusLedConfig.ShowStatus;

        public override IObservable<int> Process(IObservable<IPConnection> source)
        {
            return source.SelectStream(connection =>
            {
                var device = new global::Tinkerforge.BrickletSoundPressureLevel(Uid, connection);
                connection.Connected += (sender, e) =>
                {
                    device.SetStatusLEDConfig((byte)StatusLed);
                    device.SetConfiguration((byte)FFTSize, (byte)Weighting);
                    device.SetDecibelCallbackConfiguration(Period, false, 'x', 0, 0);
                };

                return Observable.Create<int>(observer =>
                {
                    global::Tinkerforge.BrickletSoundPressureLevel.DecibelEventHandler handler = (sender, decibel) =>
                    {
                        observer.OnNext(decibel);
                    };

                    device.DecibelCallback += handler;
                    return Disposable.Create(() =>
                    {
                        try { device.SetDecibelCallbackConfiguration(0, false, 'x', 0, 0); }
                        catch (NotConnectedException) { }
                        device.DecibelCallback -= handler;
                    });
                });
            });
        }

        public enum FftSize : byte
        {
            FftSize128 = global::Tinkerforge.BrickletSoundPressureLevel.FFT_SIZE_128,
            FftSize256 = global::Tinkerforge.BrickletSoundPressureLevel.FFT_SIZE_256,
            FftSize512 = global::Tinkerforge.BrickletSoundPressureLevel.FFT_SIZE_512,
            FftSize1024 = global::Tinkerforge.BrickletSoundPressureLevel.FFT_SIZE_1024,
        }

        public enum WeightingFunction : byte
        {
            WeightingA = global::Tinkerforge.BrickletSoundPressureLevel.WEIGHTING_A,
            WeightingB = global::Tinkerforge.BrickletSoundPressureLevel.WEIGHTING_B,
            WeightingC = global::Tinkerforge.BrickletSoundPressureLevel.WEIGHTING_C,
            WeightingD = global::Tinkerforge.BrickletSoundPressureLevel.WEIGHTING_D,
            WeightingZ = global::Tinkerforge.BrickletSoundPressureLevel.WEIGHTING_Z,
            WeightingITU = global::Tinkerforge.BrickletSoundPressureLevel.WEIGHTING_ITU_R_468
        }

        public enum BrickletSoundPressureStatusLedConfig : byte
        {
            Off = global::Tinkerforge.BrickletSoundPressureLevel.STATUS_LED_CONFIG_OFF,
            On = global::Tinkerforge.BrickletSoundPressureLevel.STATUS_LED_CONFIG_ON,
            ShowHeartbeat = global::Tinkerforge.BrickletSoundPressureLevel.STATUS_LED_CONFIG_SHOW_HEARTBEAT,
            ShowStatus = global::Tinkerforge.BrickletSoundPressureLevel.STATUS_LED_CONFIG_SHOW_STATUS
        }
    }
}
