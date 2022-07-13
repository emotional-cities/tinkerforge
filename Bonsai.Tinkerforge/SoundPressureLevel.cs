using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Tinkerforge;

namespace Bonsai.Tinkerforge
{
    /// <summary>
    /// Represents an operator that measures sound pressure level (decibels) and
    /// spectrum from a Sound Pressure Level Bricklet.
    /// </summary>
    [DefaultProperty(nameof(Uid))]
    [Description("Measures sound pressure level (decibels) and spectrum from a Sound Pressure Level Bricklet.")]
    public class SoundPressureLevel : Combinator<IPConnection, int>
    {
        /// <summary>
        /// Gets or sets the bricklet device UID.
        /// </summary>
        [TypeConverter(typeof(UidConverter))]
        [Description("The bricklet device UID.")]
        public string Uid { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the period between sample event callbacks.
        /// A value of zero disables event reporting.
        /// </summary>
        [Description("Specifies the period between sample event callbacks. A value of zero disables event reporting.")]
        public long Period { get; set; } = 1000;

        /// <summary>
        /// Gets or sets a value specifying the size, resolution and
        /// bin size of the FFT.
        /// </summary>
        [Description("Specifies the size, resolution and bin size of the FFT.")]
        public SoundPressureLevelFftSize FftSize { get; set; } = SoundPressureLevelFftSize.FftSize512;

        /// <summary>
        /// Gets or sets a value specifying decibel weighting function.
        /// </summary>
        [Description("Specifies the dB weighting function.")]
        public SoundPressureLevelWeightingFunction Weighting { get; set; } = SoundPressureLevelWeightingFunction.WeightingITU;

        /// <summary>
        /// Gets or sets a value specifying the behavior of the status LED.
        /// </summary>
        [Description("Specifies the behavior of the status LED.")]
        public SoundPressureLevelStatusLedConfig StatusLed { get; set; } = SoundPressureLevelStatusLedConfig.ShowStatus;

        /// <inheritdoc/>
        public override string ToString()
        {
            return BrickletSoundPressureLevel.DEVICE_DISPLAY_NAME;
        }

        /// <summary>
        /// Measures sound pressure level from a Sound Pressure Level Bricklet.
        /// </summary>
        /// <param name="source">
        /// A sequence containing the TCP/IP connection to the Brick Daemon.
        /// </param>
        /// <returns>the 
        /// A sequence of <see cref="int"/> values representing the
        /// sound pressure measurements from the Sound Pressure Level Bricklet
        /// in units of 1/10th of a decibel.
        /// </returns>
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
    }

    /// <summary>
    /// Specifies the size of the FFT for the Sound Pressure Level Bricklet.
    /// </summary>
    public enum SoundPressureLevelFftSize : byte
    {
        /// <summary>
        /// Specifies an FFT size of 128 samples, 64 bins, 80 samples per second.
        /// Each bin has size 320Hz.
        /// </summary>
        FftSize128 = BrickletSoundPressureLevel.FFT_SIZE_128,

        /// <summary>
        /// Specifies an FFT size of 256 samples, 128 bins, 40 samples per second.
        /// Each bin has size 160Hz.
        /// </summary>
        FftSize256 = BrickletSoundPressureLevel.FFT_SIZE_256,

        /// <summary>
        /// Specifies an FFT size of 512 samples, 256 bins, 20 samples per second.
        /// Each bin has size 80Hz.
        /// </summary>
        FftSize512 = BrickletSoundPressureLevel.FFT_SIZE_512,

        /// <summary>
        /// Specifies an FFT size of 1024 samples, 512 bins, 10 samples per second.
        /// Each bin has size 40Hz.
        /// </summary>
        FftSize1024 = BrickletSoundPressureLevel.FFT_SIZE_1024,
    }

    /// <summary>
    /// Specifies the decibel weighting function for the Sound Pressure Level Bricklet.
    /// </summary>
    public enum SoundPressureLevelWeightingFunction : byte
    {
        /// <summary>
        /// Specifies the dB(A) weighting function.
        /// </summary>
        WeightingA = BrickletSoundPressureLevel.WEIGHTING_A,

        /// <summary>
        /// Specifies the dB(B) weighting function.
        /// </summary>
        WeightingB = BrickletSoundPressureLevel.WEIGHTING_B,

        /// <summary>
        /// Specifies the dB(C) weighting function.
        /// </summary>
        WeightingC = BrickletSoundPressureLevel.WEIGHTING_C,

        /// <summary>
        /// Specifies the dB(D) weighting function.
        /// </summary>
        WeightingD = BrickletSoundPressureLevel.WEIGHTING_D,

        /// <summary>
        /// Specifies the dB(Z) weighting function (flat response).
        /// </summary>
        WeightingZ = BrickletSoundPressureLevel.WEIGHTING_Z,

        /// <summary>
        /// Specifies the ITU-R 468 standard weighting function (flat response).
        /// </summary>
        WeightingITU = BrickletSoundPressureLevel.WEIGHTING_ITU_R_468
    }

    /// <summary>
    /// Specifies the behavior of the Air Quality Bricklet status LED.
    /// </summary>
    public enum SoundPressureLevelStatusLedConfig : byte
    {
        /// <summary>
        /// The status LED will be permanently OFF.
        /// </summary>
        Off = BrickletSoundPressureLevel.STATUS_LED_CONFIG_OFF,

        /// <summary>
        /// The status LED will be permanently ON as long as the bricklet is powered.
        /// </summary>
        On = BrickletSoundPressureLevel.STATUS_LED_CONFIG_ON,

        /// <summary>
        /// The status LED will change state periodically every second.
        /// </summary>
        ShowHeartbeat = BrickletSoundPressureLevel.STATUS_LED_CONFIG_SHOW_HEARTBEAT,

        /// <summary>
        /// The LED will show communication traffic between Brick and Bricklet,
        /// flickering once for every 10 received data packets.
        /// </summary>
        ShowStatus = BrickletSoundPressureLevel.STATUS_LED_CONFIG_SHOW_STATUS
    }
}
