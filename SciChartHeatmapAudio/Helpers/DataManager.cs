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

namespace SciChartHeatmapAudio.Helpers
{
    public class DataManager
    {

        private const string ResourcePrefix = "Xamarin.Examples.Demo.Droid.";

        private const string PriceDataIndu = "Resources.Data.INDU_Daily.csv";
        private const string PriceDataEurUsd = "Resources.Data.EURUSD_Daily.csv";
        private const string TradeTicks = "Resources.Data.TradeTicks.csv";
        private const string Waveform = "Resources.Data.waveform.txt";

        public static readonly DataManager Instance = new DataManager();

        private readonly Random _random = new Random(42);

        private DataManager()
        {
        }

        public void SetHeatmapValues(double[] heatmapValues, int heatmapIndex, int heatmapWidth, int heatmapHeight, int seriesPerPeriod)
        {
            var cx = heatmapWidth / 2;
            var cy = heatmapHeight / 2;

            var angle = Math.PI * 2 * heatmapIndex / seriesPerPeriod;

            for (var x = 0; x < heatmapWidth; x++)
            {
                for (var y = 0; y < heatmapHeight; y++)
                {
                    var v = (1 + Math.Sin(x * 0.04 + angle)) * 50 + (1 + Math.Sin(y * 0.1 + angle)) * 50 * (1 + Math.Sin(angle * 2));
                    var r = Math.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    var exp = Math.Max(0, 1 - r * 0.008);

                    heatmapValues[x * heatmapHeight + y] = v * exp + _random.NextDouble() * 50;
                }
            }
        }
    }
}