using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.IO;
using Java.Lang;
using SciChartHeatmapAudio.Helpers;
using SciChartHeatmapAudio.Services;
using static SciChartHeatmapAudio.Helpers.WvlLogger;

namespace SciChartHeatmapAudio
{
    [Activity(Label = "DebugAudioToWavActivity")]
    public class DebugAudioToWavActivity : Activity
    {

        private static int RECORDER_BPP = 16;
        private static string AUDIO_RECORDER_FILE_EXT_WAV = ".wav";
        private static string AUDIO_RECORDER_FOLDER = "AudioRecorder";
        private static string AUDIO_RECORDER_TEMP_FILE = "record_temp.raw";
        private static int RECORDER_SAMPLERATE = 44100;
        //private static int RECORDER_CHANNELS = AudioFormat.CHANNEL_IN_STEREO;
        private static int RECORDER_CHANNELS = 12;
        //private static int RECORDER_AUDIO_ENCODING = AudioFormat.Enc ENCODING_PCM_16BIT;
        private static int RECORDER_AUDIO_ENCODING = 2;

        private AudioRecord recorder = null;
        private int bufferSize = 0;
        private System.Threading.Thread recordingThread = null;
        private bool isRecording = false;

        private string wavFileName;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_debugaudio_towav);

            SetButtonHandlers();
            EnableButtons(false);
            /*
            bufferSize = AudioRecord.getMinBufferSize(8000,
                AudioFormat.CHANNEL_CONFIGURATION_MONO,
                AudioFormat.ENCODING_PCM_16BIT);
            */

            bufferSize = AudioRecord.GetMinBufferSize(8000,
                Android.Media.ChannelIn.Mono,
                Android.Media.Encoding.Pcm16bit);
            WvlLogger.Log(LogType.TraceAll,"bufferSize : " + bufferSize.ToString());
        }

        #region Buttons

        private void SetButtonHandlers()
        {
            WvlLogger.Log(LogType.TraceAll,"SetButtonHandlers()");
            ((Button)FindViewById(Resource.Id.btnStart)).Click += delegate
            {
                WvlLogger.Log(LogType.TraceAll,"Start Recording clicked");
                EnableButtons(true);
                StartRecording();
            };

            ((Button)FindViewById(Resource.Id.btnStop)).Click += delegate
            {
                WvlLogger.Log(LogType.TraceAll,"Start Recording clicked");
                EnableButtons(false);
                StopRecording();
            };

            ((Button)FindViewById(Resource.Id.btnSendWav)).Click += async delegate
            {
                WvlLogger.Log(LogType.TraceAll,"Send Wav clicked");
                EnableButtons(false);
                var wvlService = new WvlService();
                if (wavFileName != "")
                {
                    await wvlService.PostAudioFile(wavFileName);
                    //var res = await wvlService.PostTest();
                }
            };
        }

        private void EnableButton(int id, bool isEnable)
        {
            WvlLogger.Log(LogType.TraceAll,"EnableButton()");
            ((Button)FindViewById(id)).Enabled = isEnable;
        }

        private void EnableButtons(bool isRecording)
        {
            WvlLogger.Log(LogType.TraceAll,"EnableButtons()");
            EnableButton(Resource.Id.btnStart, !isRecording);
            EnableButton(Resource.Id.btnStop, isRecording);
        }

        #endregion

        #region Filenames

        private string GetFilename()
        {
            WvlLogger.Log(LogType.TraceAll,"GetFilename()");
            string filepath = Android.OS.Environment.ExternalStorageDirectory.Path;
            File file = new File(filepath, AUDIO_RECORDER_FOLDER);

            if (!file.Exists())
            {
                file.Mkdirs();
            }

            var result = (file.AbsolutePath + "/" + DateTime.Now.Millisecond.ToString() + AUDIO_RECORDER_FILE_EXT_WAV);
            wavFileName = result;
            WvlLogger.Log(LogType.TraceAll,"GetFilename() : " + result);
            return result;
        }

        private string GetTempFilename()
        {
            WvlLogger.Log(LogType.TraceAll,"GetTempFilename()");
            string filepath = Android.OS.Environment.ExternalStorageDirectory.Path;
            File file = new File(filepath, AUDIO_RECORDER_FOLDER);

            if (!file.Exists())
            {
                file.Mkdirs();
            }

            File tempFile = new File(filepath, AUDIO_RECORDER_TEMP_FILE);

            if (tempFile.Exists())
                tempFile.Delete();

            var result = (file.AbsolutePath + "/" + AUDIO_RECORDER_TEMP_FILE);
            WvlLogger.Log(LogType.TraceAll,"GetTempFilename() : " + result);
            return result;
        }

        #endregion

        private void StartRecording()
        {
            WvlLogger.Log(LogType.TraceAll,"StartRecording()");

            //recorder = new AudioRecord(MediaRecorder.AudioSource.MIC,
            // RECORDER_SAMPLERATE, RECORDER_CHANNELS, RECORDER_AUDIO_ENCODING, bufferSize);

            recorder = new AudioRecord(AudioSource.Mic, RECORDER_SAMPLERATE, (ChannelIn)RECORDER_CHANNELS, (Android.Media.Encoding)RECORDER_AUDIO_ENCODING, bufferSize);

            int i = (int)recorder.State;
            if (i == 1)
                recorder.StartRecording();

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

            recordingThread = new System.Threading.Thread(new ThreadStart(
                WriteAudioDataToFile
                ));
            recordingThread.Start();
        }

        private void WriteAudioDataToFile()
        {
            WvlLogger.Log(LogType.TraceAll,"WriteAudioDataToFile()");

            byte[] data = new byte[bufferSize];
            string filename = GetTempFilename();
            FileOutputStream fos = null;

            try
            {
                fos = new FileOutputStream(filename);
            }
            catch (FileNotFoundException e)
            {
                // TODO Auto-generated catch block
                //e.printStackTrace();
                WvlLogger.Log(LogType.TraceExceptions,e.ToString());
            }

            int read = 0;

            if (null != fos)
            {
                while (isRecording)
                {
                    read = recorder.Read(data, 0, bufferSize);

                    //if (AudioRecord.ERROR_INVALID_OPERATION != read)
                    if ((int)RecordStatus.ErrorInvalidOperation != read)
                    {
                        try
                        {
                            fos.Write(data);
                        }
                        catch (IOException e)
                        {
                            //e.printStackTrace();
                            WvlLogger.Log(LogType.TraceExceptions,"WriteAudioDataToFile - Exception on os.Write() : " + e.ToString());
                        }
                    }
                }

                try
                {
                    fos.Close();
                }
                catch (IOException e)
                {
                    //e.printStackTrace();
                    WvlLogger.Log(LogType.TraceExceptions,"WriteAudioDataToFile - Exception on os.Close() : " + e.ToString());
                }
            }
        }

        private void StopRecording()
        {
            WvlLogger.Log(LogType.TraceAll,"StopRecording()");

            if (null != recorder)
            {
                isRecording = false;

                int i = (int)recorder.State;
                if (i == 1)
                    recorder.Stop();
                recorder.Release();

                recorder = null;
                recordingThread = null;
            }

            CopyWaveFile(GetTempFilename(), GetFilename());
            DeleteTempFile();
        }

        private void DeleteTempFile()
        {
            WvlLogger.Log(LogType.TraceAll,"DeleteTempFile()");
            File file = new File(GetTempFilename());

            file.Delete();
        }

        private void CopyWaveFile(string inFilename, string outFilename)
        {
            WvlLogger.Log(LogType.TraceAll,"CopyWaveFile()");

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

                WvlLogger.Log(LogType.TraceValues,"CopyWaveFile() - File size: " + totalDataLen.ToString());

                WriteWaveFileHeader(fos, totalAudioLen, totalDataLen,
                    longSampleRate, channels, byteRate);

                while (fis.Read(data) != -1){
                    fos.Write(data);
                }

                fis.Close();
                fos.Close();
            }
            catch (FileNotFoundException e)
            {
                //e.printStackTrace();
                WvlLogger.Log(LogType.TraceExceptions,"CopyWaveFile() - FileNotFoundException: " + e.ToString());
            }
            catch (IOException e)
            {
                //e.printStackTrace();
                WvlLogger.Log(LogType.TraceExceptions, "CopyWaveFile() - IOException: " + e.ToString());
            }
        }

        private void WriteWaveFileHeader(FileOutputStream fos, long totalAudioLen,
            long totalDataLen, long longSampleRate, int channels, long byteRate) 
        {
            WvlLogger.Log(LogType.TraceAll,"WriteWaveFileHeader()");
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
                WvlLogger.Log(LogType.TraceExceptions,"WriteWaveFileHeader() - Exception: " + e.ToString());
            }
    }
}


}