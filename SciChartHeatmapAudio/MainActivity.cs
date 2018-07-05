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
using Android.Content;

namespace SciChartHeatmapAudio
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var licensingContract = @"<LicenseContract>" +
                                   "</LicenseContract>";
            SciChart.Charting.Visuals.SciChartSurface.SetRuntimeLicenseKey(licensingContract);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            FindViewById<Button>(Resource.Id.debugHeatpMap).Click += delegate {
                var activity2 = new Intent(this, typeof(DebugHeatmapActivity));
                //activity2.PutExtra("MyData", "Data from Activity1");
                StartActivity(activity2);
            };

            FindViewById<Button>(Resource.Id.debugAudio).Click += delegate {
                var activity3 = new Intent(this, typeof(DebugAudioActivity));
                StartActivity(activity3);
            };

            FindViewById<Button>(Resource.Id.debugAudioWithRecording).Click += delegate {
                var activity4 = new Intent(this, typeof(DebugAudioRecActivity));
                StartActivity(activity4);
            };

            FindViewById<Button>(Resource.Id.debugAudioWavConverter).Click += delegate {
                var activity5 = new Intent(this, typeof(DebugAudioToWavActivity));
                StartActivity(activity5);
            };

            FindViewById<Button>(Resource.Id.debugAudioWavServiceConverter).Click += delegate {
                var activity6 = new Intent(this, typeof(DebugAudioToWavServiceActivity));
                StartActivity(activity6);
            };
        }

    }
}

