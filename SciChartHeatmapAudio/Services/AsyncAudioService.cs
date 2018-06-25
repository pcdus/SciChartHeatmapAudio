using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AForge.Math;
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SciChartHeatmapAudio.Helpers;

namespace SciChartHeatmapAudio.Services
{
    public class AsyncAudioService
    {
        // default
        AudioRecord audioRecord;
        public event EventHandler samplesUpdated;

        // new
        int buffer = 2048 * sizeof(byte);

        // record file
        static string filePath = "/data/data/Xamarin.Examples.Demo/files/testAudio.mp4";
        static string fileName = "testAudio.mp4";
        byte[] audioBuffer = null;
        bool endRecording = false;
        bool isRecording = false;

        #region FFT

        public int[] FFT(int[] y)
        {
            Logger.Log("FFT()");
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

        public async Task StartRecordAsync()
        {
            Logger.Log("StartRecordAsync()");
            if (audioRecord == null)
            {
                //audioRecord = new AudioRecord(AudioSource.Mic, 44100, ChannelIn.Mono, Encoding.Pcm16bit, 2048 * sizeof(byte));
                audioRecord = new AudioRecord(AudioSource.Mic, 44100, ChannelIn.Mono, Android.Media.Encoding.Pcm16bit, buffer);
                if (audioRecord.State != State.Initialized)
                    throw new InvalidOperationException("This device doesn't support AudioRecord");
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

            await SaveRecordAsync();
        }


        //public async Task StopRecordAsync()
        public void StopRecord()
        {
            Logger.Log("StopRecordAsync()");
            audioRecord.Stop();
            audioRecord = null;
        }

        void OnNext()
        {
            Logger.Log("OnNext()");
            short[] audioBuffer = new short[2048];
            audioRecord.Read(audioBuffer, 0, audioBuffer.Length);

            int[] result = new int[audioBuffer.Length];
            for (int i = 0; i < audioBuffer.Length; i++)
            {
                result[i] = (int)audioBuffer[i];
            }

            samplesUpdated(this, new SamplesUpdatedEventArgs(result));
        }

        /// <summary>
        /// Save AudioRecord in file
        /// </summary>
        /// <returns></returns>
        async Task SaveRecordAsync()
        {
            Logger.Log("SaveRecordAsync()");
            using (var fileStream = new FileStream(filePath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                while (true)
                {
                    if (endRecording)
                    {
                        endRecording = false;
                        break;
                    }
                    try
                    {
                        // Keep reading the buffer while there is audio input.
                        int numBytes = await audioRecord.ReadAsync(audioBuffer, 0, audioBuffer.Length);
                        await fileStream.WriteAsync(audioBuffer, 0, numBytes);
                        // Do something with the audio input.
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("SaveRecordAsync() - Exception : " + ex.ToString());
                        Console.Out.WriteLine(ex.Message);
                        break;
                    }
                }
                fileStream.Close();
            }
            audioRecord.Stop();
            audioRecord.Release();
            isRecording = false;

            //RaiseRecordingStateChangedEvent();
        }
    }
}