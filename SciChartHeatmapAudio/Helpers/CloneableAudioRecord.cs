using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace SciChartHeatmapAudio.Helpers
{
    //public class CloneableAudioRecord : AudioRecord, ICloneable
    public class CloneableAudioRecord : AudioRecord
    {
        /*
        object ICloneable.Clone()
        {
            return this.MemberwiseClone();
        }
        */

        public virtual CloneableAudioRecord Clone()
        {
            return (CloneableAudioRecord)this.MemberwiseClone();
        }

        public CloneableAudioRecord([GeneratedEnum] AudioSource audioSource, 
            int sampleRateInHz, 
            [GeneratedEnum] ChannelIn channelConfig, 
            [GeneratedEnum] Android.Media.Encoding audioFormat, 
            int bufferSizeInBytes) : base(audioSource, sampleRateInHz, channelConfig, audioFormat, bufferSizeInBytes)
         {

         }
    }
}