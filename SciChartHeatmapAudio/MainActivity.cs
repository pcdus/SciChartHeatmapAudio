using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using SciChart.Charting.Visuals.Axes;
using SciChart.Charting.Model.DataSeries;
using System.Threading;
using SciChart.Charting.Visuals;
using SciChart.Charting.Visuals.RenderableSeries;
using Android.Graphics;
using System.Threading.Tasks;
using SciChartHeatmapAudio.Services;
using SciChartHeatmapAudio.CustomViews;
using Java.Lang;

namespace SciChartHeatmapAudio
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    //[Activity(Label = "@string/app_name", Theme = "@style/AppTheme")]
    public class MainActivity : AppCompatActivity
    {

        // From XF Scichartshowcase
        XyDataSeries<int, int> samplesDataSeries;
        XyDataSeries<int, int> fftDataSeries;

        //FastUniformHeatmapRenderableSeries heatmapSeries;
        HeatmapRenderableSeries heatmapSeries;

        int samplesCount = 2048;

        CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        CancellationToken token;

        SciChartSurface surfaceView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var licensingContract = @"<LicenseContract>" +
              "</LicenseContract>";

            SciChart.Charting.Visuals.SciChartSurface.SetRuntimeLicenseKey(licensingContract);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            surfaceView = FindViewById<SciChartSurface>(Resource.Id.sciChartSpectrogram);

            ConfigureSpectrogramChart();

            token = cancelTokenSource.Token;

            Task.Run(() =>
            {
                var audioService = new AudioService();

                audioService.samplesUpdated += AudioService_samplesUpdated;

                audioService.StartRecord();
            }, token);
        }

        private void AudioService_samplesUpdated(object sender, System.EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("AudioService_samplesUpdated()");

            var audioService = (AudioService)sender;
            System.Diagnostics.Debug.WriteLine("AudioService_samplesUpdated() - get audioService");
            if (token.IsCancellationRequested)
            {
                System.Diagnostics.Debug.WriteLine("AudioService_samplesUpdated() - token.IsCancellationRequested");
                audioService.StopRecord();
                return;
            }

            var arguments = e as SamplesUpdatedEventArgs;
            System.Diagnostics.Debug.WriteLine("AudioService_samplesUpdated() - get arguments");

            if (arguments != null)
            {
                System.Diagnostics.Debug.WriteLine("AudioService_samplesUpdated() - arguments != null");
                var samples = arguments.UpdatedSamples;
                if (samples.Length < samplesCount)
                    return;

                //samplesDataSeries.YValues = samples;
                //samplesDataSeries.YValues.Add(samples);
                //samplesDataSeries.YValues.AddAll(samples);
                samplesDataSeries.UpdateRangeYAt(0, samples);

                var fftValues = audioService.FFT(samples);
                //fftDataSeries.YValues = fftValues;
                fftDataSeries.YValues.Add(fftValues);
                heatmapSeries.AppenData(fftValues);
                
                /*
                Device.BeginInvokeOnMainThread(sampleSurface.UpdateDataSeries);
                Device.BeginInvokeOnMainThread(fftSurface.UpdateDataSeries);
                Device.BeginInvokeOnMainThread(spectrogramSurface.UpdateDataSeries);
                */
            }


        }

        /*
        protected override void OnStart()
        {
            base.OnStart();

            //InitExample();
        }
        */

        void ConfigureSpectrogramChart()
        {
            System.Diagnostics.Debug.WriteLine("ConfigureSpectrogramChart()");

            //samplesDataSeries = new XYDataSeries<int, int> { FifoCapacity = 500000 };
            samplesDataSeries = new XyDataSeries<int, int>() { FifoCapacity = new Integer(500000) };

            // Axis
            var xAxis = new NumericAxis(this)
            {
                AutoRange = AutoRange.Always,
                DrawMajorBands = false,
                DrawLabels = false,
                DrawMajorTicks = false,
                DrawMinorTicks = false,
                DrawMajorGridLines = false,
                DrawMinorGridLines = false,
                FlipCoordinates = true,
                AxisAlignment = AxisAlignment.Left
            };
            surfaceView.XAxes.Add(xAxis);


            var yAxis = new NumericAxis(this)
            {
                AutoRange = AutoRange.Always,
                DrawMajorBands = false,
                DrawLabels = false,
                DrawMajorTicks = false,
                DrawMinorTicks = false,
                DrawMajorGridLines = false,
                DrawMinorGridLines = false,
                FlipCoordinates = true,
                AxisAlignment = AxisAlignment.Bottom
            };
            surfaceView.YAxes.Add(yAxis);


            //SciChartSurfaceView debug = new SciChartSurfaceView();

            var fastUniformHeatmapRenderableSeries = new FastUniformHeatmapRenderableSeries();

            // Create a ColorMap
            ColorMap colorMap = new ColorMap(
                new int[] { Color.Transparent, Color.DarkBlue, Color.Purple, Color.Red, Color.Yellow, Color.White },
                new float[] { 0f, 0.0001f, 0.25f, 0.50f, 0.75f, 1f }
                );
            // Apply the ColorMap
            fastUniformHeatmapRenderableSeries.ColorMap = colorMap;

            fastUniformHeatmapRenderableSeries.Maximum = 70.0;
            fastUniformHeatmapRenderableSeries.Minimum = -30.0;

            surfaceView.RenderableSeries.Add(fastUniformHeatmapRenderableSeries);

        }
    }
}

