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

        int debugMin = 0;
        int debugMax = 0;

        // new
        //int buffer = 2048 * sizeof(byte);
        int buffer = 1024 * sizeof(byte);

        #region FFT

        public int[] FFT(int[] y)
        {
            WvlLogger.Log(LogType.TraceAll,"FFT()");
            WvlLogger.Log(LogType.TraceValues, "FFT() - int[] y.length : " + y.Length.ToString() + "y.Sum : " + y.Sum().ToString());


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


            // debug
            /*
            int myMin = 0;
            int myMax = 0;
            myMin = result.Min();
            myMax = result.Max();
            debugMin = (debugMin <= myMin) ? debugMin : myMin;
            debugMax = (debugMax >= myMax) ? debugMax : myMax;
            */

            WvlLogger.Log(LogType.TraceValues, "FFT() - result : " + result.Length.ToString() + "y.Sum : " + result.Sum().ToString());
            return result;


        }

        /*
        public double[] CalculateFFT(byte[] signal)
        {
            int mNumberOfFFTPoints = 1024;
            double mMaxFFTSample;

            double temp;
            Complex[] y;
            Complex[] complexSignal = new Complex[mNumberOfFFTPoints];
            double[] absSignal = new double[mNumberOfFFTPoints / 2];

            for (int i = 0; i < mNumberOfFFTPoints; i++)
            {
                temp = (double)((signal[2 * i] & 0xFF) | (signal[2 * i + 1] << 8)) / 32768.0F;
                complexSignal[i] = new Complex(temp, 0.0);
            }

            y = FFT.fft(complexSignal); // --> Here I use FFT class

            mMaxFFTSample = 0.0;
            mPeakPos = 0;
            for (int i = 0; i < (mNumberOfFFTPoints / 2); i++)
            {
                absSignal[i] = Math.Sqrt(Math.Pow(y[i].Re, 2) + Math.Pow(y[i].Im, 2));
                if (absSignal[i] > mMaxFFTSample)
                {
                    mMaxFFTSample = absSignal[i];
                    mPeakPos = i;
                }
            }

            return absSignal;

        }
        */

        #endregion

        #region AudioRecord : Start/Stop/OnNext 

        public void StartRecord()
        {
            WvlLogger.Log(LogType.TraceAll,"StartRecord()");
            if (audioRecord == null)
            {

                //audioRecord = new AudioRecord(AudioSource.Mic, 44100, ChannelIn.Mono, Encoding.Pcm16bit, 2048 * sizeof(byte));
                //audioRecord = new AudioRecord(AudioSource.Mic, 44100, ChannelIn.Mono, Android.Media.Encoding.Pcm16bit, buffer);
                audioRecord = new AudioRecord(AudioSource.Mic, 48000, ChannelIn.Mono, Android.Media.Encoding.Pcm16bit, buffer);
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
            //short[] audioBuffer = new short[2048];
            short[] audioBuffer = new short[1024];
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

}