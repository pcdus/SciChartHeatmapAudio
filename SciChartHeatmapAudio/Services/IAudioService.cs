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

namespace SciChartHeatmapAudio.Services
{
    public interface IAudioService
    {
        event EventHandler samplesUpdated;
        void StartRecord();
        void StopRecord();
        void PlayRecod();
        int[] FFT(int[] y);
    }

    public class SamplesUpdatedEventArgs : EventArgs
    {
        public int[] UpdatedSamples { get; set; }

        public SamplesUpdatedEventArgs(int[] samples)
        {
            UpdatedSamples = samples;
        }
    }
}