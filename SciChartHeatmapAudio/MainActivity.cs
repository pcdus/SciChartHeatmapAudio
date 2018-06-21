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
using SciChart.Core.Model;
using System.Linq;


namespace SciChartHeatmapAudio
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    //[Activity(Label = "@string/app_name", Theme = "@style/AppTheme")]
    public class MainActivity : AppCompatActivity
    {

        // From XF Scichartshowcase
        XyDataSeries<int, int> samplesDataSeries;
        XyDataSeries<int, int> fftDataSeries;

        //HeatmapRenderableSeries heatmapSeries;
        //FastUniformHeatmapRenderableSeries heatmapSeries;
        //UniformHeatmapDataSeries<int, int, double> heatmapSeries = new UniformHeatmapDataSeries<int, int, double>(400, 400);
        //UniformHeatmapDataSeries heatmapSeries = new UniformHeatmapDataSeries<int, int, int>(1024, 1024);
        //private readonly UniformHeatmapDataSeries<int, int, double> heatmapSeries = new UniformHeatmapDataSeries<int, int, double>(1024, 1024);
        private readonly UniformHeatmapDataSeries<int, int, int> heatmapSeries = new UniformHeatmapDataSeries<int, int, int>(1024, 1024);

        int samplesCount = 2048;

        CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        CancellationToken token;

        SciChartSurface surfaceView;

        // From XF Scichartshowcase Converter
        //UniformHeatmapDataSeries<int, int, int> spectrogramDataSeries;
        // X index - value of last added element into X values
        int lastElement = 0;


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
                //samplesDataSeries.UpdateRangeYAt(0, samples);
                UpdateSamplesDataSeries(samples);
                /*
                var xArray = new int[samples.Length];
                for (int i = 0; i < samples.Length; i++)
                {
                    xArray[i] = lastElement++;
                }
                samplesDataSeries.Append(xArray, samples);
                */

                var fftValues = audioService.FFT(samples);

                //fftDataSeries.YValues = fftValues;
                //fftDataSeries.YValues.Add(fftValues);
                UpdateFFTDataSeries(fftValues);
                /*
                var xArray = new int[fftValues.Length];
                for (int i = 0; i < fftValues.Length; i++)
                {
                    xArray[i] = i;
                }

                try
                {
                    if (fftDataSeries.Count == 0)
                        fftDataSeries.Append(xArray, fftValues);
                    else
                        fftDataSeries.UpdateRangeYAt(0, fftValues);
                }
                catch(Exception exc)
                {
                    System.Diagnostics.Debug.WriteLine("AudioService_samplesUpdated() - Exception : " + exc.ToString());
                }
                */

                //heatmapSeries.AppenData(fftValues);
                //heatmapSeries.Update(fftValues);
                heatmapSeries.UpdateZValues(fftValues);

                /*
                Device.BeginInvokeOnMainThread(sampleSurface.UpdateDataSeries);
                Device.BeginInvokeOnMainThread(fftSurface.UpdateDataSeries);
                Device.BeginInvokeOnMainThread(spectrogramSurface.UpdateDataSeries);
                */
            }


        }


        #region from SciChartSurfaceRenderer

        //private void UpdateSamplesDataSeries(XYDataSeries<int, int> dataSeries)
        private void UpdateSamplesDataSeries(int[] dataSeries)
        {
            System.Diagnostics.Debug.WriteLine("UpdateSamplesDataSeries()");
            /*
            var xArray = new int[dataSeries.YValues.Length];
            for (int i = 0; i < dataSeries.YValues.Length; i++)
            {
                xArray[i] = lastElement++;
            }
            samplesDataSeries.Append(xArray, dataSeries.YValues);
            System.Diagnostics.Debug.WriteLine("UpdateSamplesDataSeries() - xarray : " + xArray.Length.ToString() + " - YValues : " + dataSeries.YValues.Sum().ToString());
            */
            var xArray = new int[dataSeries.Length];
            for (int i = 0; i < dataSeries.Length; i++)
            {
                xArray[i] = lastElement++;
            }
            samplesDataSeries.Append(xArray, dataSeries);
            System.Diagnostics.Debug.WriteLine("UpdateSamplesDataSeries() - xarray : " + xArray.Length.ToString() + " - YValues : " + dataSeries.Sum().ToString());
        }


        //private void UpdateFFTDataSeries(XYDataSeries<int, int> dataSeries)
        private void UpdateFFTDataSeries(int[] dataSeries)
        {
            System.Diagnostics.Debug.WriteLine("UpdateFFTDataSeries()");

            /*
            var xArray = new int[dataSeries.YValues.Length];
            for (int i = 0; i < dataSeries.YValues.Length; i++)
            {
                xArray[i] = i;
            }

            if (fftDataSeries.Count == 0)
                fftDataSeries.Append(xArray, dataSeries.YValues);
            else
                fftDataSeries.UpdateRangeYAt(0, dataSeries.YValues);
            */
            var xArray = new int[dataSeries.Length];
            for (int i = 0; i < dataSeries.Length; i++)
            {
                xArray[i] = i;
            }

            try
            {
                if (fftDataSeries.Count == 0)
                {
                    fftDataSeries.Append(xArray, dataSeries);
                    System.Diagnostics.Debug.WriteLine("UpdateSamplesDataSeries() - xarray : " + xArray.Length.ToString() + " - YValues : " + dataSeries.Sum().ToString());
                }
                else
                {
                    fftDataSeries.UpdateRangeYAt(0, dataSeries);
                    System.Diagnostics.Debug.WriteLine("UpdateSamplesDataSeries() - 0 - YValues : " + dataSeries.Sum().ToString());
                }
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine("UpdateFFTDataSeries() - Exception : " + exc.ToString());
            }
        }

        #endregion

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
            fftDataSeries = new XyDataSeries<int, int>() { FifoCapacity = new Integer(500000) };
            
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
            /*
            ColorMap colorMap = new ColorMap(
                new int[] { Color.Transparent, Color.DarkBlue, Color.Purple, Color.Red, Color.Yellow, Color.White },
                new float[] { 0f, 0.0001f, 0.25f, 0.50f, 0.75f, 1f }
                );
            */
            ColorMap colorMap = new ColorMap(
                new int[] { Color.Transparent, Color.DarkBlue, Color.Purple, Color.Red, Color.Yellow, Color.White },
                new float[] { 0f, 0.2f, 0.4f, 0.6f, 0.8f, 1f }
                );

            // Apply the ColorMap
            fastUniformHeatmapRenderableSeries.ColorMap = colorMap;
            fastUniformHeatmapRenderableSeries.DataSeries = heatmapSeries;
            fastUniformHeatmapRenderableSeries.Maximum = 70.0;
            fastUniformHeatmapRenderableSeries.Minimum = -30.0;

            surfaceView.RenderableSeries.Add(fastUniformHeatmapRenderableSeries);

        }
    }
}

