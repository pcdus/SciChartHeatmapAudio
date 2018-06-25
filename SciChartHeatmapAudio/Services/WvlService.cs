using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.IO;
using SciChartHeatmapAudio.Helpers;

namespace SciChartHeatmapAudio.Services
{
    public class WvlService
    {

        public string url = "";


        public async Task PostAudioFile(string fileName)
        {
            Logger.Log("PostAudioFile() - fileName : " + fileName);
            using (HttpClient client = new HttpClient())
            {
                
                //Stream stream = await File.OpenAsync(FileAccess.ReadWrite);
                //var file = File.Open(fileName, FileMode.Open, FileAccess.Read);
                /*
                Java.IO.File file = new Java.IO.File(fileName);
                FileInputStream fileInputStream = new FileInputStream(file);

                var content = new MultipartFormDataContent();
                content.Add(new StreamContent(fileInputStream), "AudioFile", fileName);
                content.Add(new StreamContent())

                HttpResponseMessage response = await client.PostAsync(url, content);

                await response.Content.ReadAsStringAsync();
                */

                try
                {
                    Logger.Log("PostAudioFile() - Create client.DefaultRequestHeaders");
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("multipart/form-data"));
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "multipart/form-data");
                    //client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                    Logger.Log("PostAudioFile() - Deserialize file");
                    byte[] b = System.IO.File.ReadAllBytes(fileName);

                    Logger.Log("PostAudioFile() - MultipartFormDataContent");
                    MultipartFormDataContent formData = new MultipartFormDataContent();
                    formData.Add(new StringContent("rotating_machine"), "machine_type");
                    formData.Add(new StringContent("My machine"), "machine_name");
                    formData.Add(new StringContent("My campaign"), "campaign");
                    formData.Add(new StringContent("Pierre-Christophe"), "user_name");
                    // formData.Add(new ByteArrayContent(b, 0, b.Length), "audio_file");

                    Logger.Log("PostAudioFile() - client.PostAsync()");
                    var responseObj = await client.PostAsync(url, formData);

                    Logger.Log("PostAudioFile() - Response : " + responseObj.ToString());
                }
                catch (Exception ex)
                {
                    Logger.Log("PostAudioFile() - Exception : " + ex.ToString());
                }

            }
        }

        /// <summary>
        /// Another test of WebService call
        /// (https://stackoverflow.com/questions/21569770/wrong-content-type-header-generated-using-multipartformdatacontent)
        /// </summary>
        /// <returns></returns>
        public async Task<string> PostTest()
        {
            Logger.Log("PostTest()");
            string servResp = "";
            Logger.Log("PostTest() - MultipartFormDataContent");
            using (var content = new MultipartFormDataContent())
            {
                Logger.Log("PostTest() - MultipartFormDataContent.Headers : " + content.Headers.ToString());
                
                //content.Headers.Clear();
                //content.Headers.Remove("Content-Type");
                //content.Headers.TryAddWithoutValidation("Content-Type", "multipart/form-data;");
                content.Add(new StringContent("rotating_machine"), "machine_type");
                content.Add(new StringContent("My machine"), "machine_name");
                content.Add(new StringContent("My campaign"), "campaign");
                content.Add(new StringContent("Pierre-Christophe"), "user_name");

                Logger.Log("PostTest() - HttpClientHandler");
                HttpClientHandler handler = new HttpClientHandler();
                var cookieContainer = new CookieContainer();
                handler.CookieContainer = cookieContainer;

                Logger.Log("PostTest() - HttpRequestMessage");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "");
                request.Headers.ExpectContinue = false;
                request.Content = content;

                try
                {
                    Logger.Log("PostTest() - HttpClient");
                    var httpClient = new HttpClient(handler);
                    HttpResponseMessage response = await httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    servResp = await response.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    Logger.Log("PostTest() - Exception : " + ex.ToString());
                }
            }

            return servResp;
        }
    }
}