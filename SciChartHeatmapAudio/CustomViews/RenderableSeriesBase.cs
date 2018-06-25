using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals.RenderableSeries;

namespace SciChartHeatmapAudio.CustomViews
{

    public enum RenderableSeriesType
    {
        Line,
        Column,
        Heatmap
    }

    public abstract class RenderableSeriesBase
    {
        public RenderableSeriesType SeriesType { get; set; }
        public DataSeries DataSeries { get; set; }
    }

    public class LineRenderableSeries : RenderableSeriesBase
    {
        public new XyDataSeries<int, int> DataSeries { get; set; }
        public Color Stroke { get; set; }
        public float StrokeThickness { get; set; }

        public LineRenderableSeries()
        {
            SeriesType = RenderableSeriesType.Line;
        }
    }


    public class HeatmapRenderableSeries : RenderableSeriesBase
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public int[] Data { get; set; }
        public ColorMap ColorMap { get; set; }

        public HeatmapRenderableSeries(int width, int height)
        {
            Width = width;
            Height = height;

            Data = new int[Width * Height];

            SeriesType = RenderableSeriesType.Heatmap;
        }

        public void AppenData(int[] data)
        {
            var spectrogramSize = Width * Height;
            var fftSize = data.Length;
            var offset = spectrogramSize - fftSize;

            Array.Copy(Data, fftSize, Data, 0, offset);
            Array.Copy(data, 0, Data, offset, fftSize);
        }

        public void UpdateValues(int[] data)
        {
            Data = data;
        }
    }
}