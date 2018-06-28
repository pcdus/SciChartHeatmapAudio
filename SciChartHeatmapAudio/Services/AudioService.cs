using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AForge.Math;
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SciChartHeatmapAudio.Helpers;

using static SciChartHeatmapAudio.Helpers.WvlLogger;

namespace SciChartHeatmapAudio.Services
{
    //public class AudioService : IAudioService
    public class AudioService
    {
        // default
        AudioRecord audioRecord;
        public event EventHandler samplesUpdated;

        // new
        int buffer = 2048 * sizeof(byte);

        #region FFT

        public int[] FFT(int[] y)
        {
            WvlLogger.Log(LogType.TraceAll,"FFT()");
            var input = new AForge.Math.Complex[y.Length];

            for (int i = 0; i < y.Length; i++)
            {
                input[i] = new AForge.Math.Complex(y[i], 0);
            }

            FourierTransform.FFT(input, FourierTransform.Direction.Forward);

            var result = new int[y.Length / 2];

            // getting magnitude
            for (int i = 0; i < y.Length / 2 - 1; i++)
            {
                var current = Math.Sqrt(input[i].Re * input[i].Re + input[i].Im * input[i].Im);
                current = Math.Log10(current) * 10;
                result[i] = (int)current;
            }

            return result;
        }

        #endregion

        #region AudioRecord : Start/Stop/OnNext 

        public void StartRecord()
        {
            WvlLogger.Log(LogType.TraceAll,"StartRecord()");
            if (audioRecord == null)
            {

                //audioRecord = new AudioRecord(AudioSource.Mic, 44100, ChannelIn.Mono, Encoding.Pcm16bit, 2048 * sizeof(byte));
                audioRecord = new AudioRecord(AudioSource.Mic, 44100, ChannelIn.Mono, Android.Media.Encoding.Pcm16bit, buffer);
                WvlLogger.Log(LogType.TraceAll, "StartRecord() - AudioRecord : " + AudioSource.Mic.ToString() + 
                                                            " - SampleRateInHz : 44100" + 
                                                            " - ChannelIn : " + ChannelIn.Mono.ToString() + 
                                                            " - Encoding : " + Android.Media.Encoding.Pcm16bit.ToString() + 
                                                            " - buffer : "  + buffer.ToString());
                if (audioRecord.State != State.Initialized)
                {
                    WvlLogger.Log(LogType.TraceExceptions,  "StartRecord() - InvalidOperationException : This device doesn't support AudioRecord");
                    throw new InvalidOperationException("This device doesn't support AudioRecord");
                }
            }

            //audioRecord.SetRecordPositionUpdateListener()

            audioRecord.StartRecording();

            while (audioRecord.RecordingState == RecordState.Recording)
            {
                try
                {
                    OnNext();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public void StopRecord()
        {
            WvlLogger.Log(LogType.TraceAll,"StopRecord()");
            audioRecord.Stop();
            audioRecord = null;
        }

        void OnNext()
        {
            WvlLogger.Log(LogType.TraceAll,"OnNext()");
            short[] audioBuffer = new short[2048];
            WvlLogger.Log(LogType.TraceValues, "OnNext() - audioRecord.Read - audioBuffer.Length : " + audioBuffer.Length.ToString());
            audioRecord.Read(audioBuffer, 0, audioBuffer.Length);

            int[] result = new int[audioBuffer.Length];

            for (int i = 0; i < audioBuffer.Length; i++)
            {
                result[i] = (int)audioBuffer[i];
            }

            samplesUpdated(this, new SamplesUpdatedEventArgs(result));
        }

        #endregion

    }

    public class SamplesUpdatedEventArgs : EventArgs
    {
        public int[] UpdatedSamples { get; set; }

        public SamplesUpdatedEventArgs(int[] samples)
        {
            WvlLogger.Log(LogType.TraceAll,"SamplesUpdatedEventArgs()");
            WvlLogger.Log(LogType.TraceValues, "SamplesUpdatedEventArgs() - samples : " + samples.Sum().ToString());
            UpdatedSamples = samples;
        }
    }
}