using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.IO;
using Java.Lang;
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Modifiers;
using SciChart.Charting.Visuals;
using SciChart.Charting.Visuals.Axes;
using SciChart.Charting.Visuals.RenderableSeries;
using SciChart.Data.Model;
using SciChartHeatmapAudio.Helpers;
using SciChartHeatmapAudio.Services;
using static SciChartHeatmapAudio.Helpers.WvlLogger;

namespace SciChartHeatmapAudio
{
    [Activity(Label = "DebugAudioToWavServiceActivity")]
    public class DebugAudioToWavServiceActivity : Activity
    {

        // AudioRecord
        private AudioRecord recorder = null;
        private int bufferSize = 0;
        CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        CancellationToken token;
        WavAudioService wavAudioService;
        //int samplesCount = 2048;
        int samplesCount = 1024;

        // bools
        private bool isRecording = false;
        private bool isPlaying = false;
        private bool hasRecord = false;

        //
        private string wavFileName;

        // SciChart Surfaces
        public SciChartSurface Surface => FindViewById<SciChartSurface>(Resource.Id.firstChartRec);
        public SciChartSurface HeatmapSurface => FindViewById<SciChartSurface>(Resource.Id.secondChartRec);

        // DataSeries
        XyDataSeries<int, int> samplesDataSeries;
        XyDataSeries<int, int> fftDataSeries;
        UniformHeatmapDataSeries<int, int, int> heatmapSeries = new UniformHeatmapDataSeries<int, int, int>(width, height);
        int lastElement = 0;

        // HeatMap
        //public static int width = 1024;
        //public static int height = 1024;
        public static int width = 512;
        public static int height = 512;
        public int[] Data = new int[width * height];

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_debugaudio_towav);

            InitCharts();
            SetButtonHandlers();
            EnableButtons(false);
                        
            // bufferSize (returns 2048 // not used)
            bufferSize = AudioRecord.GetMinBufferSize(8000,
                Android.Media.ChannelIn.Mono,
                Android.Media.Encoding.Pcm16bit);
        }

        #region Buttons

        private void SetButtonHandlers()
        {
            WvlLogger.Log(LogType.TraceAll,"SetButtonHandlers()");
            ((Button)FindViewById(Resource.Id.btnStart)).Click += async delegate
            {
                WvlLogger.Log(LogType.TraceAll,"Start Recording clicked");
                isRecording = true;
                EnableButtons(true);
                // StartRecording();
                wavAudioService = new WavAudioService();
                wavAudioService.samplesUpdated += AudioService_samplesUpdated;
                wavAudioService.StartRecording();
            };

            // Stop record or play
            ((Button)FindViewById(Resource.Id.btnStop)).Click += delegate
            {
                WvlLogger.Log(LogType.TraceAll,"Stop clicked");
                if (isRecording)
                {
                    WvlLogger.Log(LogType.TraceAll,"isRecording : " + isRecording.ToString());
                    isRecording = false;
                    // StopRecording();
                    wavAudioService.StopRecording();
                    wavAudioService.samplesUpdated -= AudioService_samplesUpdated;
                    hasRecord = true;
                }
                if (isPlaying)
                {
                    WvlLogger.Log(LogType.TraceAll,"isPlaying : " + isPlaying.ToString());
                    isPlaying = false;
                    wavAudioService.StopPlaying();
                }
                EnableButtons(false);
            };

            // Play record
            ((Button)FindViewById(Resource.Id.btnPlay)).Click += async delegate
            {
                WvlLogger.Log(LogType.TraceAll,"Start Playing clicked");
                isPlaying = true;
                EnableButtons(true);
                // StopRecording();
                await wavAudioService.StartPlaying();
                //wavAudioService.samplesUpdated -= AudioService_samplesUpdated;
                isPlaying = false;
                EnableButtons(false);
            };

            // Send wav to API
            ((Button)FindViewById(Resource.Id.btnSendWav)).Click += async delegate
            {
                WvlLogger.Log(LogType.TraceAll,"Send Wav clicked");
                EnableButtons(false);
                var wvlService = new WvlService();
                if (wavFileName != "")
                {
                    //await wvlService.PostAudioFile(wavFileName);
                    await wvlService.PostAudioFile();
                    //var res = await wvlService.PostTest();
                }
            };
        }

        private void EnableButton(int id, bool isEnable)
        {
            WvlLogger.Log(LogType.TraceAll,"EnableButton()");
            var button = ((Button)FindViewById(id));
            WvlLogger.Log(LogType.TraceValues,"Button : " + button.ToString() + " - isEnable : " + isEnable.ToString());
            //((Button)FindViewById(id)).Enabled = isEnable;
            button.Enabled = isEnable;
        }

        private void EnableButtons(bool state)
        {
            WvlLogger.Log(LogType.TraceAll,"EnableButtons()");
            WvlLogger.Log(LogType.TraceValues, "isRecording : " + isRecording.ToString() + " - isPlaying : " + isPlaying.ToString());
            if (isRecording || isPlaying)
            {
                WvlLogger.Log(LogType.TraceAll,"EnableButtons : case 1 ");
                EnableButton(Resource.Id.btnStart, !state);
                EnableButton(Resource.Id.btnStop, state);
                if (hasRecord)
                    EnableButton(Resource.Id.btnPlay, !state);
                else
                    EnableButton(Resource.Id.btnPlay, false);
            }
            else
            {
                WvlLogger.Log(LogType.TraceAll,"EnableButtons : case 2 ");
                EnableButton(Resource.Id.btnStart, !state);
                EnableButton(Resource.Id.btnStop, state);
                if (hasRecord)
                {
                    EnableButton(Resource.Id.btnPlay, !state);
                    EnableButton(Resource.Id.btnSendWav, !state);
                }
                else
                {
                    EnableButton(Resource.Id.btnPlay, state);
                    EnableButton(Resource.Id.btnSendWav, state);
                }

            }
        }

        #endregion

        #region SciChart

        #region -> Charts

        private void InitCharts()
        {
            InitFirstChart();
            InitSecondChart();
        }


        private void InitFirstChart()
        {
            WvlLogger.Log(LogType.TraceAll,"InitFirstChart()");

            var xAxis = new NumericAxis(this)
            {
                AutoRange = AutoRange.Always,
                DrawMajorBands = false,
                DrawLabels = false,
                DrawMajorTicks = false,
                DrawMinorTicks = false,
                DrawMajorGridLines = false,
                DrawMinorGridLines = false,
                AxisTitle = "Time (seconds)"
            };

            var yAxis = new NumericAxis(this)
            {
                AutoRange = AutoRange.Never,
                VisibleRange = new DoubleRange(short.MinValue, short.MaxValue),
                DrawMajorBands = false,
                DrawLabels = false,
                DrawMajorTicks = false,
                DrawMinorTicks = false,
                DrawMajorGridLines = false,
                DrawMinorGridLines = false,
                AxisTitle = "Freq (hZ)"
            };

            samplesDataSeries = new XyDataSeries<int, int> { FifoCapacity = new Java.Lang.Integer(500000) };
            var rs = new FastLineRenderableSeries { DataSeries = samplesDataSeries };

            using (Surface.SuspendUpdates())
            {
                Surface.XAxes.Add(xAxis);
                Surface.YAxes.Add(yAxis);
                Surface.RenderableSeries.Add(rs);
            }
        }

        private void InitSecondChart()
        {
            WvlLogger.Log(LogType.TraceAll,"InitSecondChart()");

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

            var yAxis = new NumericAxis(this)
            {
                DrawMajorBands = false,
                DrawLabels = false,
                DrawMajorTicks = false,
                DrawMinorTicks = false,
                DrawMajorGridLines = false,
                DrawMinorGridLines = false,
                FlipCoordinates = true,
                AxisAlignment = AxisAlignment.Bottom,
                VisibleRange = new DoubleRange(height / 2, height),
            };

            // from XF sample
            /*
            var rs = new FastUniformHeatmapRenderableSeries
            {
                DataSeries = heatmapSeries,
                ColorMap = new SciChart.Charting.Visuals.RenderableSeries.ColorMap(
                    new int[] { Color.Transparent, Color.DarkBlue, Color.Purple, Color.Red, Color.Yellow, Color.White },
                    new float[] { 0f, 0.2f, 0.4f, 0.6f, 0.8f, 1f }
                )
            };
           */

            // from Android sample
            var rs = new FastUniformHeatmapRenderableSeries
            {
                DataSeries = heatmapSeries,
                Maximum = 30,
                Minimum = -40,
                ColorMap = new SciChart.Charting.Visuals.RenderableSeries.ColorMap(
                    new int[] { Color.Transparent, Color.DarkBlue, Color.Purple, Color.Red, Color.Yellow, Color.White },
                    new float[] { 0f, 0.0001f, 0.25f, 0.50f, 0.75f, 1f }
                )
            };

            #region Zoom and Pan

            // Create interactivity modifiers
            var pinchZoomModifier = new PinchZoomModifier();
            pinchZoomModifier.SetReceiveHandledEvents(true);

            var zoomPanModifier = new ZoomPanModifier();
            zoomPanModifier.SetReceiveHandledEvents(true);

            var zoomExtentsModifier = new ZoomExtentsModifier();
            zoomExtentsModifier.SetReceiveHandledEvents(true);

            var yAxisDragModifier = new YAxisDragModifier();
            yAxisDragModifier.SetReceiveHandledEvents(true);

            // Create modifier group from declared modifiers
            var modifiers = new ModifierGroup(pinchZoomModifier, zoomPanModifier, zoomExtentsModifier, yAxisDragModifier);

            #endregion

            using (HeatmapSurface.SuspendUpdates())
            {
                HeatmapSurface.XAxes.Add(xAxis);
                HeatmapSurface.YAxes.Add(yAxis);
                HeatmapSurface.RenderableSeries.Add(rs);
                //HeatmapSurface.ChartModifiers.Add(modifiers);
            }
        }

        #endregion 

        #region -> Series (SampleData, FFTData, HeatMap)

        private void UpdateSamplesDataSeries(int[] dataSeries)
        {
            WvlLogger.Log(LogType.TraceAll,"UpdateSamplesDataSeries()");

            var xArray = new int[dataSeries.Length];
            for (int i = 0; i < dataSeries.Length; i++)
            {
                xArray[i] = lastElement++;
            }
            try
            {
                samplesDataSeries.Append(xArray, dataSeries);
                WvlLogger.Log(LogType.TraceValues, "UpdateSamplesDataSeries() - xarray : " + xArray.Length.ToString() + " - dataSeries : " + dataSeries.Sum().ToString());
            }
            catch (System.Exception e)
            {
                WvlLogger.Log(LogType.TraceExceptions,"UpdateSamplesDataSeries() - exception : " + e.ToString());
            }
        }

        private void UpdateFFTDataSeries(int[] fftValues)
        {
            WvlLogger.Log(LogType.TraceAll,"UpdateFFTDataSeries()");

            var xArray = new int[fftValues.Length];
            for (int i = 0; i < fftValues.Length; i++)
            {
                xArray[i] = i;
            }

            try
            {
                if (fftDataSeries.Count == 0)
                {
                    fftDataSeries.Append(xArray, fftValues);
                    WvlLogger.Log(LogType.TraceValues, "UpdateSamplesDataSeries() - xarray : " + xArray.Length.ToString() + " - fftValues : " + fftValues.Sum().ToString());
                }
                else
                {
                    fftDataSeries.UpdateRangeYAt(0, fftValues);
                    WvlLogger.Log(LogType.TraceValues, "UpdateSamplesDataSeries() - 0 - fftValues : " + fftValues.Sum().ToString());
                }
            }
            catch (System.Exception e)
            {
                WvlLogger.Log(LogType.TraceExceptions,"UpdateFFTDataSeries() - exception : " + e.ToString());
            }
        }

        //public void AppenData(int[] data)
        public void UpdateHeatmapDataSeries(int[] data)
        //public async void UpdateHeatmapDataSeries(int[] data)
        {
            WvlLogger.Log(LogType.TraceAll,"UpdateHeatmapDataSeries()");
            
            //WvlLogger.Log(LogType.TraceAll,"UpdateHeatmapDataSeries - Width : " + width.ToString() + " - Height : " + height.ToString());
            WvlLogger.Log(LogType.TraceValues, "UpdateHeatmapDataSeries() - Data before Array.Copy() : " + Data.Sum().ToString());

            var spectrogramSize = width * height;
            var fftSize = data.Length;
            var offset = spectrogramSize - fftSize;
            WvlLogger.Log(LogType.TraceValues, "UpdateHeatmapDataSeries() - set offset : " + offset.ToString());

            try
            {                
                Array.Copy(Data, fftSize, Data, 0, offset);
                Array.Copy(data, 0, Data, offset, fftSize);
                WvlLogger.Log(LogType.TraceValues, "UpdateHeatmapDataSeries() - Data after Array.Copy() : " + Data.Sum().ToString());

                heatmapSeries.UpdateZValues(Data);
                WvlLogger.Log(LogType.TraceAll, "UpdateSamplesDataSeries() - UpdateZValues()");
                
            }
            catch (System.Exception e)
            {
                WvlLogger.Log(LogType.TraceExceptions,"UpdateSamplesDataSeries() - exception : " + e.ToString());
            }
            
            /*
            try
            {
                heatmapSeries.UpdateZValues(data);
            }
            catch (System.Exception e)
            {
                WvlLogger.Log(LogType.TraceExceptions, "UpdateSamplesDataSeries() - exception : " + e.ToString());
            }
            */
        }


        #endregion

        #endregion

        private void AudioService_samplesUpdated(object sender, System.EventArgs e)
        //private async void AudioService_samplesUpdated(object sender, System.EventArgs e)
        {
            WvlLogger.Log(LogType.TraceAll,"AudioService_samplesUpdated()");
            
            var audioService = (WavAudioService)sender;

            if (token.IsCancellationRequested)
            {
                WvlLogger.Log(LogType.TraceAll,"AudioService_samplesUpdated() - token.IsCancellationRequested");
                audioService.StopRecording();
                return;
            }

            var arguments = e as SamplesUpdatedEventArgs;
            
            if (arguments != null)
            {
                WvlLogger.Log(LogType.TraceAll,"AudioService_samplesUpdated() - arguments != null");
                var samples = arguments.UpdatedSamples;

                /*
                //if (samples.Length < samplesCount)
                if (samples.Length < (samplesCount / 2))
                {
                    WvlLogger.Log(LogType.TraceValues, "AudioService_samplesUpdated() - samples.Length < samplesCount - sample.Length : " + samples.Length.ToString() + " samplesCount : " + samplesCount.ToString());
                    return;
                }
                */

                WvlLogger.Log(LogType.TraceValues, "AudioService_samplesUpdated() - sample.Length : " + samples.Length.ToString());


                /*
                //samplesDataSeries.YValues = samples;
                //UpdateSamplesDataSeries(samples);

                var fftValues = await audioService.FFT(samples);
                
                //fftDataSeries.YValues = fftValues;
                //UpdateFFTDataSeries(fftValues);
                */
                //heatmapSeries.AppenData(fftValues);
                //UpdateHeatmapDataSeries(fftValues);
                //UpdateHeatmapDataSeries(samples);

                
                System.Threading.Thread thread = new System.Threading.Thread(() => UpdateHeatmapDataSeries(samples));
                thread.Priority = System.Threading.ThreadPriority.Highest;
                thread.Start();
                
            }
            
        }
    }


}