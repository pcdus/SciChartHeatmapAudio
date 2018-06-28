using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

using static SciChartHeatmapAudio.Helpers.WvlLogger;

namespace SciChartHeatmapAudio.Services
{
    public class AsyncAudioService
    {
        // default
        AudioRecord audioRecord;
        public event EventHandler samplesUpdated;

        // new
        int bufferSize = 2048 * sizeof(byte);
        byte[] buffer;

        // record file
        //static string filePath = "/data/data/SciChartHeatmapAudio.SciChartHeatmapAudio/files/testAudio.mp4";
        static string fileName = "record_temp.raw";
        private static string AUDIO_RECORDER_TEMP_FILE = "record_temp.raw";
        private static string AUDIO_RECORDER_FOLDER = "AudioRecorder";


        //byte[] audioBuffer = null;
        byte[] audioBuffer = new byte[2048];
        bool endRecording = false;
        bool isRecording = false;

        AudioTrack audioTrack = null;

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

        public async Task StartRecordAsync()
        {
            WvlLogger.Log(LogType.TraceAll,"StartRecordAsync()");
            if (audioRecord == null)
            {
                //audioRecord = new AudioRecord(AudioSource.Mic, 44100, ChannelIn.Mono, Encoding.Pcm16bit, 2048 * sizeof(byte));
                audioRecord = new AudioRecord(AudioSource.Mic, 44100, ChannelIn.Mono, Android.Media.Encoding.Pcm16bit, bufferSize);
                if (audioRecord.State != State.Initialized)
                {
                    WvlLogger.Log(LogType.TraceExceptions, "This device doesn't support AudioRecord");
                    throw new InvalidOperationException("This device doesn't support AudioRecord");
                }
            }

            //audioRecord.SetRecordPositionUpdateListener()

            audioRecord.StartRecording();

            // stop button debug
            /*
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
            */
            
            await SaveRecordAsync();
        }


        //public async Task StopRecordAsync()
        public void StopRecord()
        {
            WvlLogger.Log(LogType.TraceAll,"StopRecordAsync()");
            endRecording = true;
            Thread.Sleep(250); // Give it time to drop out.
            /*
            audioRecord.Stop();
            audioRecord = null;
            */
        }

        void OnNext()
        {
            WvlLogger.Log(LogType.TraceAll,"OnNext()");
            short[] audioBuffer = new short[2048];
            audioRecord.Read(audioBuffer, 0, audioBuffer.Length);

            int[] result = new int[audioBuffer.Length];
            for (int i = 0; i < audioBuffer.Length; i++)
            {
                result[i] = (int)audioBuffer[i];
            }

            // stop button debug
            /*
            samplesUpdated(this, new SamplesUpdatedEventArgs(result));
            */
        }

        /// <summary>
        /// Save AudioRecord in file
        /// </summary>
        /// <returns></returns>
        async Task SaveRecordAsync()
        {
            WvlLogger.Log(LogType.TraceAll,"SaveRecordAsync()");
            var filePath = GetTempFilename();
            using (var fileStream = new FileStream(filePath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                while (true)
                {
                    WvlLogger.Log(LogType.TraceAll,"SaveRecordAsync() - while true");
                    if (endRecording)
                    {
                        endRecording = false;
                        break;
                    }
                    try
                    {
                        // default
                        /*
                        // Keep reading the buffer while there is audio input.
                        int numBytes = await audioRecord.ReadAsync(audioBuffer, 0, audioBuffer.Length);
                        //await fileStream.WriteAsync(audioBuffer, 0, numBytes);
                        fileStream.Write(audioBuffer, 0, numBytes);
                        // Do something with the audio input.
                        */

                        // custom
                        //short[] audioBuffer = new short[2048];
                        // Keep reading the buffer while there is audio input.
                        int numBytes = await audioRecord.ReadAsync(audioBuffer, 0, audioBuffer.Length);
                        WvlLogger.Log(LogType.TraceValues, "SaveRecordAsync() - audioRecord.ReadAsync() - audioBuffer.Length : " + audioBuffer.Length.ToString() +
                                                                                                       " - numBytes : " + numBytes.ToString());
                        await fileStream.WriteAsync(audioBuffer, 0, numBytes);

                        byte[] fileAudioBuffer = new byte[audioBuffer.Length * sizeof(short)];
                        WvlLogger.Log(LogType.TraceValues, "SaveRecordAsync() - audioRecord.ReadAsync() - fileAudioBuffer.Length : " + fileAudioBuffer.Length.ToString());
                        // Do something with the audio input.


                        
                        // OnNext
                        //OnNext();
                        /*
                        WvlLogger.Log(LogType.TraceAll,"EmbeddedOnNext()");
                        int[] result = new int[audioBuffer.Length];
                        for (int i = 0; i < audioBuffer.Length; i++)
                        {
                            result[i] = (int)audioBuffer[i];
                        }
                        samplesUpdated(this, new SamplesUpdatedEventArgs(result));
                        */
                    }
                    catch (Exception ex)
                    {
                        WvlLogger.Log(LogType.TraceExceptions,"SaveRecordAsync() - Exception : " + ex.ToString());
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

        private string GetTempFilename()
        {
            WvlLogger.Log(LogType.TraceAll, "GetTempFilename()");
            string filepath = Android.OS.Environment.ExternalStorageDirectory.Path;
            Java.IO.File file = new Java.IO.File(filepath, AUDIO_RECORDER_FOLDER);

            if (!file.Exists())
            {
                file.Mkdirs();
            }

            Java.IO.File tempFile = new Java.IO.File(filepath, AUDIO_RECORDER_TEMP_FILE);

            if (tempFile.Exists())
                tempFile.Delete();

            var result = (file.AbsolutePath + "/" + AUDIO_RECORDER_TEMP_FILE);
            WvlLogger.Log(LogType.TraceAll, "GetTempFilename() : " + result);
            return result;
        }

        #region -> Playing

        public async Task StartPlaying()
        {
            WvlLogger.Log(LogType.TraceAll, "StartPlaying()");

            string filePath = GetTempFilename();
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            long totalBytes = new System.IO.FileInfo(filePath).Length;
            buffer = binaryReader.ReadBytes((Int32)totalBytes);
            fileStream.Close();
            fileStream.Dispose();
            binaryReader.Close();

            WvlLogger.Log(LogType.TraceValues, "StartPlaying() - " +
                   " fileStream: " + fileStream.Name +
                   " totalBytes: " + totalBytes.ToString());
            await PlayAudioTrackAsync();
        }

        protected async Task PlayAudioTrackAsync()
        {
            WvlLogger.Log(LogType.TraceAll, "PlayAudioTrackAsync()");
            WvlLogger.Log(LogType.TraceValues, "PlayAudioTrackAsync() - buffer.Length : " + buffer.Length.ToString());

            audioTrack = new AudioTrack(
                // Stream type
                Android.Media.Stream.Music,
                // Frequency
                44100,
                // Mono or stereo
                ChannelOut.Mono,
                // Audio encoding
                Android.Media.Encoding.Pcm16bit,
                // Length of the audio clip.
                buffer.Length,
                // Mode. Stream or static.
                AudioTrackMode.Stream);

            try
            {
                audioTrack.Play();
            }
            catch (Exception ex)
            {
                WvlLogger.Log(LogType.TraceAll, "PlayAudioTrackAsync() - audioTrack.Play() excetion : " + ex.ToString());
            }

            //await audioTrack.WriteAsync(buffer, 0, buffer.Length);
            await audioTrack.WriteAsync(buffer, 0, buffer.Length);
        }

        public void StopPlaying()
        {
            WvlLogger.Log(LogType.TraceAll, "StopPlaying()");
            if (audioTrack != null)
            {
                audioTrack.Stop();
                audioTrack.Release();
                audioTrack = null;
            }
        }

        #endregion
    }
}