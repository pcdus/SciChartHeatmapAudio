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

namespace SciChartHeatmapAudio.Services
{
    public class AudioService : IAudioService
    {
        AudioRecord audioRecord;

        public event EventHandler samplesUpdated;

        public int[] FFT(int[] y)
        {
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

        public void StartRecord()
        {
            System.Diagnostics.Debug.WriteLine("StartRecord()");
            if (audioRecord == null)
            {
                audioRecord = new AudioRecord(AudioSource.Mic, 44100, ChannelIn.Mono, Android.Media.Encoding.Pcm16bit, 2048 * sizeof(byte));
                if (audioRecord.State != State.Initialized)
                {
                    System.Diagnostics.Debug.WriteLine("StartRecord() - This device doesn't support AudioRecord");
                    throw new InvalidOperationException("This device doesn't support AudioRecord");
                }
            }

            //audioRecord.SetRecordPositionUpdateListener()
            System.Diagnostics.Debug.WriteLine("StartRecord() - audioRecord.StartRecording()");
            audioRecord.StartRecording();

            while (audioRecord.RecordingState == RecordState.Recording)
            {
                try
                {
                    OnNext();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("StartRecord() - Recording exception");
                    throw ex;
                }
            }
        }

        public void StopRecord()
        {
            System.Diagnostics.Debug.WriteLine("StopRecord()");
            audioRecord.Stop();
            audioRecord = null;
        }

        public void PlayRecod()
        {

        }

        void OnNext()
        {
            System.Diagnostics.Debug.WriteLine("OnNext()");
            short[] audioBuffer = new short[2048];
            audioRecord.Read(audioBuffer, 0, audioBuffer.Length);

            int[] result = new int[audioBuffer.Length];
            for (int i = 0; i < audioBuffer.Length; i++)
            {
                result[i] = (int)audioBuffer[i];
            }

            samplesUpdated(this, new SamplesUpdatedEventArgs(result));
        }

    }
}