using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.IO;
using SciChartHeatmapAudio.Helpers;

namespace SciChartHeatmapAudio.Services
{
    public class WavAudioService
    {
        // default
        
        public event EventHandler samplesUpdated;

        // AudiRecord
        AudioRecord audioRecord;
        private static int RECORDER_BPP = 16;
        private static int RECORDER_SAMPLERATE = 44100;
        //private static int RECORDER_CHANNELS = AudioFormat.CHANNEL_IN_STEREO;        
        //private static int RECORDER_CHANNELS = 12;
        //private static int RECORDER_CHANNELS = AudioFormat.CHANNEL_IN_MONO;        
        private static int RECORDER_CHANNELS = 16;
        //private static int RECORDER_AUDIO_ENCODING = AudioFormat.Enc ENCODING_PCM_16BIT;
        private static int RECORDER_AUDIO_ENCODING = 2;

        // Bools
        bool isRecording = false;

        // Files
        private static string AUDIO_RECORDER_FILE_EXT_WAV = ".wav";
        private static string AUDIO_RECORDER_FOLDER = "AudioRecorder";
        private static string AUDIO_RECORDER_TEMP_FILE = "record_temp.raw";
        private string wavFileName;


        private System.Threading.Thread recordingThread = null;
        int bufferSize = 2048 * sizeof(byte);
        byte[] buffer;
        AudioTrack audioTrack = null;

        #region AudioRecord Recording / Playing

        #region -> Recording

        //public void StartRecording()
        public async Task StartRecording()
        {
            
            Logger.Log("StartRecording()");

            //recorder = new AudioRecord(MediaRecorder.AudioSource.MIC,
            // RECORDER_SAMPLERATE, RECORDER_CHANNELS, RECORDER_AUDIO_ENCODING, bufferSize);

            audioRecord = new AudioRecord(
                AudioSource.Mic, 
                RECORDER_SAMPLERATE, 
                (ChannelIn)RECORDER_CHANNELS, 
                (Android.Media.Encoding)RECORDER_AUDIO_ENCODING, 
                bufferSize);

            /*
            int i = (int)audioRecord.State;
            if (i == 1)
            */
            if (audioRecord.State == State.Initialized)
                audioRecord.StartRecording();

            isRecording = true;

            /*
            recordingThread = new Thread(new Runnable() {
            @Override
            public void run()
            {
                writeAudioDataToFile();
            }
            },"AudioRecorder Thread");
		    recordingThread.start();
            */

            System.Threading.Thread newThread = new System.Threading.Thread(new ThreadStart(
                WriteAudioDataToFile
                ));
            newThread.Start();
        }

        public void StopRecording()
        {
            Logger.Log("StopRecording()");
            
            if (null != audioRecord)
            {
                isRecording = false;
                /*
                int i = (int)audioRecord.State;
                if (i == 1)
                */
                if (audioRecord.State == State.Initialized)
                    audioRecord.Stop();
                audioRecord.Release();

                audioRecord = null;
                recordingThread = null;
            }

            CopyWaveFile(GetTempFilename(), GetFilename());
            //DeleteTempFile();
        }

        #endregion

        #region -> Playing

        public async Task StartPlaying()
        {
            Logger.Log("StartPlaying()");

            string filePath = GetTempFilename();
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            long totalBytes = new System.IO.FileInfo(filePath).Length;
            buffer = binaryReader.ReadBytes((Int32)totalBytes);
            fileStream.Close();
            fileStream.Dispose();
            binaryReader.Close();
            await PlayAudioTrackAsync();
        }

        protected async Task PlayAudioTrackAsync()
        {
            Logger.Log("PlayAudioTrackAsync");
            audioTrack = new AudioTrack(
                // Stream type
                Android.Media.Stream.Music,
                // Frequency
                RECORDER_SAMPLERATE,
                // Mono or stereo
                ChannelOut.Mono,
                // Audio encoding
                Android.Media.Encoding.Pcm16bit,
                // Length of the audio clip.
                buffer.Length,
                // Mode. Stream or static.
                AudioTrackMode.Stream);

            audioTrack.Play();

            await audioTrack.WriteAsync(buffer, 0, buffer.Length);
        }

        public void StopPlaying()
        {
            Logger.Log("StopPlaying()");
            if (audioTrack != null)
            {
                audioTrack.Stop();
                audioTrack.Release();
                audioTrack = null;
            }
        }

        #endregion

        #endregion

        #region Files 

        #region -- Files creation/copy/delete

        private void WriteAudioDataToFile()
        {
            Logger.Log("WriteAudioDataToFile()");

            byte[] data = new byte[bufferSize];
            string filename = GetTempFilename();
            FileOutputStream fos = null;

            try
            {
                fos = new FileOutputStream(filename);
            }
            catch (Java.IO.FileNotFoundException e)
            {
                // TODO Auto-generated catch block
                //e.printStackTrace();
                Logger.Log(e.ToString());
            }

            int read = 0;

            if (null != fos)
            {
                while (isRecording)
                {
                    read = audioRecord.Read(data, 0, bufferSize);

                    //if (AudioRecord.ERROR_INVALID_OPERATION != read)
                    if ((int)RecordStatus.ErrorInvalidOperation != read)
                    {
                        try
                        {
                            fos.Write(data);
                        }
                        catch (Java.IO.IOException e)
                        {
                            //e.printStackTrace();
                            Logger.Log("WriteAudioDataToFile - Exception on fos.Write() : " + e.ToString());
                        }
                    }
                }

                try
                {
                    fos.Close();
                }
                catch (Java.IO.IOException e)
                {
                    //e.printStackTrace();
                    Logger.Log("WriteAudioDataToFile - Exception on fos.Close() : " + e.ToString());
                }
            }
        }

        private void DeleteTempFile()
        {
            Logger.Log("DeleteTempFile()");
            Java.IO.File file = new Java.IO.File(GetTempFilename());

            file.Delete();
        }

        private void CopyWaveFile(string inFilename, string outFilename)
        {
            Logger.Log("CopyWaveFile()");

            FileInputStream fis = null;
            FileOutputStream fos = null;


            long totalAudioLen = 0;
            long totalDataLen = totalAudioLen + 36;
            long longSampleRate = RECORDER_SAMPLERATE;
            int channels = 2;
            long byteRate = RECORDER_BPP * RECORDER_SAMPLERATE * channels / 8;

            byte[] data = new byte[bufferSize];

            try
            {
                fis = new FileInputStream(inFilename);
                fos = new FileOutputStream(outFilename);
                totalAudioLen = fis.Channel.Size();
                totalDataLen = totalAudioLen + 36;

                Logger.Log("CopyWaveFile() - File size: " + totalDataLen.ToString());

                WriteWaveFileHeader(fos, totalAudioLen, totalDataLen,
                    longSampleRate, channels, byteRate);

                while (fis.Read(data) != -1)
                {
                    fos.Write(data);
                }

                fis.Close();
                fos.Close();
            }
            catch (Java.IO.FileNotFoundException e)
            {
                //e.printStackTrace();
                Logger.Log("CopyWaveFile() - FileNotFoundException: " + e.ToString());
            }
            catch (Java.IO.IOException e)
            {
                //e.printStackTrace();
                Logger.Log("CopyWaveFile() - IOException: " + e.ToString());
            }
        }

        private void WriteWaveFileHeader(FileOutputStream fos, long totalAudioLen,
            long totalDataLen, long longSampleRate, int channels, long byteRate)
        {
            Logger.Log("WriteWaveFileHeader()");
            try
            {
                byte[] header = new byte[44];

                header[0] = (byte)'R'; // RIFF/WAVE header
                header[1] = (byte)'I';
                header[2] = (byte)'F';
                header[3] = (byte)'F';
                header[4] = (byte)(totalDataLen & 0xff);
                header[5] = (byte)((totalDataLen >> 8) & 0xff);
                header[6] = (byte)((totalDataLen >> 16) & 0xff);
                header[7] = (byte)((totalDataLen >> 24) & 0xff);
                header[8] = (byte)'W';
                header[9] = (byte)'A';
                header[10] = (byte)'V';
                header[11] = (byte)'E';
                header[12] = (byte)'f'; // 'fmt ' chunk
                header[13] = (byte)'m';
                header[14] = (byte)'t';
                header[15] = (byte)' ';
                header[16] = 16; // 4 bytes: size of 'fmt ' chunk
                header[17] = 0;
                header[18] = 0;
                header[19] = 0;
                header[20] = 1; // format = 1
                header[21] = 0;
                header[22] = (byte)channels;
                header[23] = 0;
                header[24] = (byte)(longSampleRate & 0xff);
                header[25] = (byte)((longSampleRate >> 8) & 0xff);
                header[26] = (byte)((longSampleRate >> 16) & 0xff);
                header[27] = (byte)((longSampleRate >> 24) & 0xff);
                header[28] = (byte)(byteRate & 0xff);
                header[29] = (byte)((byteRate >> 8) & 0xff);
                header[30] = (byte)((byteRate >> 16) & 0xff);
                header[31] = (byte)((byteRate >> 24) & 0xff);
                header[32] = (byte)(2 * 16 / 8); // block align
                header[33] = 0;
                header[34] = (byte)RECORDER_BPP; // bits per sample
                header[35] = 0;
                header[36] = (byte)'d';
                header[37] = (byte)'a';
                header[38] = (byte)'t';
                header[39] = (byte)'a';
                header[40] = (byte)(totalAudioLen & 0xff);
                header[41] = (byte)((totalAudioLen >> 8) & 0xff);
                header[42] = (byte)((totalAudioLen >> 16) & 0xff);
                header[43] = (byte)((totalAudioLen >> 24) & 0xff);

                fos.Write(header, 0, 44);
            }
            catch (System.Exception e)
            {
                Logger.Log("WriteWaveFileHeader() - Exception: " + e.ToString());
            }
        }

        #endregion

        #region -- Filenames

        private string GetFilename()
        {
            Logger.Log("GetFilename()");
            string filepath = Android.OS.Environment.ExternalStorageDirectory.Path;
            Java.IO.File file = new Java.IO.File(filepath, AUDIO_RECORDER_FOLDER);

            if (!file.Exists())
            {
                file.Mkdirs();
            }

            var result = (file.AbsolutePath + "/" + DateTime.Now.Millisecond.ToString() + AUDIO_RECORDER_FILE_EXT_WAV);
            wavFileName = result;
            Logger.Log("GetFilename() : " + result);
            return result;
        }

        private string GetTempFilename()
        {
            Logger.Log("GetTempFilename()");
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
            Logger.Log("GetTempFilename() : " + result);
            return result;
        }

        #endregion

        #endregion

    }
}