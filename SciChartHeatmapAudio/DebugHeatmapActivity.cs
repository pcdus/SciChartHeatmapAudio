using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals;
using SciChart.Charting.Visuals.Axes;
using SciChart.Charting.Visuals.RenderableSeries;
using SciChart.Core.Model;
using SciChartHeatmapAudio.Helpers;

namespace SciChartHeatmapAudio
{
    [Activity(Label = "DebugHeatmapActivity")]
    //[Activity(Label = "DebugHeatmapActivity", MainLauncher = true)]
    public class DebugHeatmapActivity : Activity
    {

        //private SciChartSurface Surface => View.FindViewById<SciChartSurface>(Resource.Id.chartHeatmap);
        private SciChartSurface Surface;

        private const int Width = 300;
        private const int Height = 200;
        private const int SeriesPerPeriod = 30;

        private volatile bool _isRunning = false;
        private readonly object _syncRoot = new object();
        private readonly Timer _timer = new Timer(40) { AutoReset = true };

        private int _timerIndex = 0;
        private readonly UniformHeatmapDataSeries<int, int, double> _dataSeries = new UniformHeatmapDataSeries<int, int, double>(Width, Height);

        private static readonly List<IValues<double>> ValuesList = new List<IValues<double>>(SeriesPerPeriod);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var licensingContract = @"<LicenseContract>" +
              "</LicenseContract>";

            SciChart.Charting.Visuals.SciChartSurface.SetRuntimeLicenseKey(licensingContract);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_debugheatmap);

            /*
            surfaceView = FindViewById<SciChartSurface>(Resource.Id.sciChartSpectrogram);
            */
            Surface = FindViewById<SciChartSurface>(Resource.Id.chartHeatmap);

            // replacement of OnStart() overriding
            InitExample();

            Task.Run(() =>
            {
                var array = new double[Width * Height];

                for (var i = 0; i < SeriesPerPeriod; i++)
                {
                    DataManager.Instance.SetHeatmapValues(array, i, Width, Height, SeriesPerPeriod);
                    var doubleValues = new DoubleValues(array);

                    lock (ValuesList)
                    {
                        ValuesList.Add(doubleValues);
                    }
                }
            });

        }

        /*
        protected override void OnStart()
        {
            base.OnStart();

            InitExample();
        }
        */

        protected void InitExample()
        {
            var xAxis = new NumericAxis(this);
            var yAxis = new NumericAxis(this);

            var rs = new FastUniformHeatmapRenderableSeries
            {
                ColorMap = new ColorMap(new[] { Color.DarkBlue, Color.CornflowerBlue, Color.DarkGreen, Color.Chartreuse, Color.Yellow, Color.Red }, new[] { 0, 0.2f, 0.4f, 0.6f, 0.8f, 1 }),
                Minimum = 0,
                Maximum = 200,
                DataSeries = _dataSeries,
            };

            Surface.XAxes.Add(xAxis);
            Surface.YAxes.Add(yAxis);
            Surface.RenderableSeries.Add(rs);

            Start();
        }

        private void Start()
        {
            if (_isRunning) return;

            _isRunning = true;
            _timer.Elapsed += OnTick;
            _timer.Start();
        }

        private void OnTick(object sender, ElapsedEventArgs e)
        {
            lock (_syncRoot)
            {
                if (!_isRunning) return;

                UpdateDataSeries(_timerIndex);

                _timerIndex++;
            }
        }

        private void UpdateDataSeries(int index)
        {
            var values = ValuesList[index % ValuesList.Count];
            _dataSeries.UpdateZValues(values);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Stop();

        }

        private void Stop()
        {
            if (!_isRunning) return;

            _isRunning = false;
            _timer.Stop();
            _timer.Elapsed -= OnTick;
        }

        /*
        public override void InitExampleForUiTest()
        {
            base.InitExampleForUiTest();

            lock (_syncRoot)
            {
                Stop();

                UpdateDataSeries(0);
            }
        }
        */
    }
}