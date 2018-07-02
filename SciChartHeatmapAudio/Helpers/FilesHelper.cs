using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using static SciChartHeatmapAudio.Helpers.WvlLogger;

namespace SciChartHeatmapAudio.Helpers
{
    public static class FilesHelper
    {

        // Files
        private static string AUDIO_RECORDER_FILE_EXT_WAV = ".wav";
        private static string AUDIO_RECORDER_FOLDER = "AudioRecorder";
        private static string AUDIO_RECORDER_TEMP_FILE = "record_temp.raw";
        private static string AUDIO_RECORDER_FINAL_FILE = "record.wav";
        private static string AUDIO_RECORDER_PATH = "/data/data/WavelyApp.WavelyApp/files/";

        public static string GetFilename()
        {
            WvlLogger.Log(LogType.TraceAll, "GetFilename()");

            string filepath = Android.OS.Environment.ExternalStorageDirectory.Path;
            //string filepath = Android.OS.Environment.DataDirectory.Path;
            Java.IO.File file = new Java.IO.File(filepath, AUDIO_RECORDER_FOLDER);
            //Java.IO.File file = new Java.IO.File(AUDIO_RECORDER_PATH);
            if (!file.Exists())
            {
                file.Mkdirs();
            }

            //var result = (file.AbsolutePath + "/" + DateTime.Now.Millisecond.ToString() + AUDIO_RECORDER_FILE_EXT_WAV);
            var result = (file.AbsolutePath + "/" + AUDIO_RECORDER_FINAL_FILE);
            WvlLogger.Log(LogType.TraceAll, "GetFilename() : " + result);
            return result;
        }

        public static string GetTempFilename()
        {
            WvlLogger.Log(LogType.TraceAll, "GetTempFilename()");

            string filepath = Android.OS.Environment.ExternalStorageDirectory.Path;
            //string filepath = Android.OS.Environment.DataDirectory.Path;
            Java.IO.File file = new Java.IO.File(filepath, AUDIO_RECORDER_FOLDER);
            //Java.IO.File file = new Java.IO.File(AUDIO_RECORDER_PATH);
            if (!file.Exists())
            {
                file.Mkdirs();
            }

            Java.IO.File tempFile = new Java.IO.File(filepath, AUDIO_RECORDER_TEMP_FILE);
            //Java.IO.File tempFile = new Java.IO.File(AUDIO_RECORDER_PATH, AUDIO_RECORDER_TEMP_FILE);

            if (tempFile.Exists())
                tempFile.Delete();

            var result = (file.AbsolutePath + "/" + AUDIO_RECORDER_TEMP_FILE);
            WvlLogger.Log(LogType.TraceAll, "GetTempFilename() : " + result);
            return result;
        }
    }
}