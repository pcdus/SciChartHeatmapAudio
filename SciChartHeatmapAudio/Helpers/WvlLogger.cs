using System;
using System.Diagnostics;

namespace SciChartHeatmapAudio.Helpers
{
    public static class WvlLogger
    {
        public enum LogType
        {
            TraceAll,
            TraceValues,
            TraceExceptions
        }

        public static void Log(LogType type, string param)
        {
            if ((int)type >= (int)LogType.TraceValues)
                Debug.WriteLine("Log : " + DateTime.Now.ToString("hh.mm.ss.ffffff") + " - " + param);
        }
    }
}