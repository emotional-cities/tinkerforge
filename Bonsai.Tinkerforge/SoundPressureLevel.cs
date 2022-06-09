using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    [DefaultProperty(nameof(Uid))]
    [Description("Measures sound pressure level (decibels) and spectrum from a Sound Pressure Level Bricklet.")]
    public class SoundPressureLevel : Combinator<IPConnection, int>
    {
        [TypeConverter(typeof(UidConverter))]
        [Description("The unique bricklet device UID.")]
        public string Uid { get; set; }

        [Description("Specifies the period between sample event callbacks. A value of zero disables event reporting.")]
        public long Period { get; set; } = 1000;

        [Description("Specifies the size, resolution and bin size of the FFT.")]
        public FftSizeConfig FftSize { get; set; } = FftSizeConfig.FftSize512;

        [Description("Specifies the dB weighting function.")]
        public WeightingFunction Weighting { get; set; } = WeightingFunction.WeightingITU;

        [Description("Specifies the behavior of the status LED.")]
        public BrickletSoundPressureStatusLedConfig StatusLed { get; set; } = BrickletSoundPressureStatusLedConfig.ShowStatus;

        public override IObservable<int> Process(IObservable<IPConnection> source)
        {
            return source.SelectStream((Func<IPConnection, IObservable<int>>)(connection =>
            {
                var device = new BrickletSoundPressureLevel(Uid, connection);
                connection.Connected += (sender, e) =>
                {
                    device.SetStatusLEDConfig((byte)StatusLed);
                    device.SetConfiguration((byte)this.FftSize, (byte)Weighting);
                    device.SetDecibelCallbackConfiguration(Period, false, 'x', 0, 1);
                };

                return Observable.Create<int>(observer =>
                {
                    BrickletSoundPressureLevel.DecibelEventHandler handler = (BrickletSoundPressureLevel sender, int decibel) =>
                    {
                        observer.OnNext(decibel);
                    };

                    device.DecibelCallback += handler;
                    return Disposable.Create(() =>
                    {
                        try { device.SetDecibelCallbackConfiguration(0, false, 'x', 0, 1); }
                        catch (NotConnectedException) { }
                        device.DecibelCallback -= handler;
                    });
                });
            }));
        }

        public enum FftSizeConfig : byte
        {
            FftSize128 = BrickletSoundPressureLevel.FFT_SIZE_128,
            FftSize256 = BrickletSoundPressureLevel.FFT_SIZE_256,
            FftSize512 = BrickletSoundPressureLevel.FFT_SIZE_512,
            FftSize1024 = BrickletSoundPressureLevel.FFT_SIZE_1024,
        }

        public enum WeightingFunction : byte
        {
            WeightingA = BrickletSoundPressureLevel.WEIGHTING_A,
            WeightingB = BrickletSoundPressureLevel.WEIGHTING_B,
            WeightingC = BrickletSoundPressureLevel.WEIGHTING_C,
            WeightingD = BrickletSoundPressureLevel.WEIGHTING_D,
            WeightingZ = BrickletSoundPressureLevel.WEIGHTING_Z,
            WeightingITU = BrickletSoundPressureLevel.WEIGHTING_ITU_R_468
        }

        public enum BrickletSoundPressureStatusLedConfig : byte
        {
            Off = BrickletSoundPressureLevel.STATUS_LED_CONFIG_OFF,
            On = BrickletSoundPressureLevel.STATUS_LED_CONFIG_ON,
            ShowHeartbeat = BrickletSoundPressureLevel.STATUS_LED_CONFIG_SHOW_HEARTBEAT,
            ShowStatus = BrickletSoundPressureLevel.STATUS_LED_CONFIG_SHOW_STATUS
        }
    }
}
