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
using SciChartHeatmapAudio.Helpers;
using static SciChartHeatmapAudio.Helpers.WvlLogger;

namespace SciChartHeatmapAudio.Services
{
    public class SamplesUpdatedEventArgs : EventArgs
    {
        public int[] UpdatedSamples { get; set; }

        public SamplesUpdatedEventArgs(int[] samples)
        {
            //WvlLogger.Log(LogType.TraceAll, "SamplesUpdatedEventArgs()");
            //WvlLogger.Log(LogType.TraceValues, "SamplesUpdatedEventArgs() - samples : " + samples.Sum().ToString());
            UpdatedSamples = samples;
        }
    }
}