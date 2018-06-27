using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.IO;
using Java.Lang;
using SciChartHeatmapAudio.Helpers;
using SciChartHeatmapAudio.Services;

namespace SciChartHeatmapAudio
{
    [Activity(Label = "DebugAudioToWavServiceActivity")]
    public class DebugAudioToWavServiceActivity : Activity
    {

        private AudioRecord recorder = null;
        private int bufferSize = 0;
        private System.Threading.Thread recordingThread = null;

        private bool isRecording = false;
        private bool isPlaying = false;
        private bool hasRecord = false;

        private string wavFileName;

        WavAudioService wavAudioService;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_debugaudio_towav);

            SetButtonHandlers();
            EnableButtons(false);
            /*
            bufferSize = AudioRecord.getMinBufferSize(8000,
                AudioFormat.CHANNEL_CONFIGURATION_MONO,
                AudioFormat.ENCODING_PCM_16BIT);
            */

            // returns 2048
            bufferSize = AudioRecord.GetMinBufferSize(8000,
                Android.Media.ChannelIn.Mono,
                Android.Media.Encoding.Pcm16bit);
        }

        #region Buttons

        private void SetButtonHandlers()
        {
            Logger.Log("SetButtonHandlers()");
            ((Button)FindViewById(Resource.Id.btnStart)).Click += async delegate
            {
                Logger.Log("Start Recording clicked");
                isRecording = true;
                EnableButtons(true);
                // StartRecording();
                wavAudioService = new WavAudioService();
                //wavAudioService.samplesUpdated += AudioService_samplesUpdated;
                wavAudioService.StartRecording();
            };

            // Stop record or play
            ((Button)FindViewById(Resource.Id.btnStop)).Click += delegate
            {
                Logger.Log("Stop clicked");
                if (isRecording)
                {
                    Logger.Log("isRecording : " + isRecording.ToString());
                    isRecording = false;
                    // StopRecording();
                    wavAudioService.StopRecording();
                    //wavAudioService.samplesUpdated -= AudioService_samplesUpdated;
                    hasRecord = true;
                }
                if (isPlaying)
                {
                    Logger.Log("isPlaying : " + isPlaying.ToString());
                    isPlaying = false;
                    wavAudioService.StopPlaying();
                }
                EnableButtons(false);
            };

            // Play record
            ((Button)FindViewById(Resource.Id.btnPlay)).Click += async delegate
            {
                Logger.Log("Start Playing clicked");
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
                Logger.Log("Send Wav clicked");
                EnableButtons(false);
                var wvlService = new WvlService();
                if (wavFileName != "")
                {
                    await wvlService.PostAudioFile(wavFileName);
                    //var res = await wvlService.PostTest();
                }
            };
        }

        private void EnableButton(int id, bool isEnable)
        {
            Logger.Log("EnableButton()");
            var button = ((Button)FindViewById(id));
            Logger.Log("Button : " + button.ToString() + " - isEnable : " + isEnable.ToString());
            //((Button)FindViewById(id)).Enabled = isEnable;
            button.Enabled = isEnable;
        }

        private void EnableButtons(bool state)
        {
            Logger.Log("EnableButtons()");
            Logger.Log("isRecording : " + isRecording.ToString() + " - isPlaying : " + isPlaying.ToString());
            if (isRecording || isPlaying)
            {
                Logger.Log("EnableButtons : case 1 ");
                EnableButton(Resource.Id.btnStart, !state);
                EnableButton(Resource.Id.btnStop, state);
                if (hasRecord)
                    EnableButton(Resource.Id.btnPlay, !state);
                else
                    EnableButton(Resource.Id.btnPlay, false);
            }
            else
            {
                Logger.Log("EnableButtons : case 2 ");
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


}


}