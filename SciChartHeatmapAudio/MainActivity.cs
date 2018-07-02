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
                                    "  <Customer>Wavely SAS</Customer>" +
                                    "  <OrderId>ABT180621-9335-68143</OrderId>" +
                                    "  <LicenseCount>1</LicenseCount>" +
                                    "  <IsTrialLicense>false</IsTrialLicense>" +
                                    "  <SupportExpires>06/21/2019 00:00:00</SupportExpires>" +
                                    "  <ProductCode>SC-ANDROID-2D-PRO</ProductCode>" +
                                    "  <KeyCode>b2ada3942fbba06603077724d60afd5ac74c7a88a0346aae6b4b3f4adc0fe8dc1cb3775164aa8bd460217ffd43acfa1d75d13e132f2c986bb3f602fc67583c4b9b883cb1cfb7b41e090646a95a2bdb704c2ea0f14a2790c85390f25acbdc212fbef5f225571c04bc00b1a64f428efce7e13adb97b7efa43020f8309bc4e0a61ba070450adee9f56435b7898ce6cd3ccc8e5c8277462898703bbe6d044c63cf3d01383d1ebbc762</KeyCode>" +
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

