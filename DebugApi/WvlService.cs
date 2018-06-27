using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;


namespace DebugApi
{
    public class WvlService
    {

        public string url = "http://51.254.131.100/api/recordings/";
        string fileName = @"C:\Users\pcdus\Hourrapps\OneDrive - Hourrapps\Documents\--Hourrapps\Clients\WeLoveDevs\Alexis Vlandas\SciChart support\808.wav";

        public async Task PostAudioFile(string file)
        {
            file = fileName;           
            Logger.Log("PostAudioFile() - fileName : " + fileName);
            using (HttpClient client = new HttpClient())
            {
                
                /*
                Stream stream = await File.OpenAsync(FileAccess.ReadWrite);
                var file = File.Open(fileName, FileMode.Open, FileAccess.Read);
                */
                 
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
                    /*
                    Logger.Log("PostAudioFile() - Create client.DefaultRequestHeaders");
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("multipart/form-data"));
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "multipart/form-data");
                    */
                    //client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                    Logger.Log("PostAudioFile() - MultipartFormDataContent");

                    byte[] b1 = FileToByteArray(fileName);
                    byte[] b2 = FileToByteArray2(fileName);


                    MultipartFormDataContent formData = new MultipartFormDataContent();
                    formData.Add(new StringContent("rotating_machine"), "machine_type");
                    formData.Add(new StringContent("My machine"), "machine_name");
                    formData.Add(new StringContent("My campaign"), "campaign_name");
                    formData.Add(new StringContent("Pierre-Christophe"), "user_name");
                    formData.Add(new ByteArrayContent(b1, 0, b1.Length), "audio_file", "audio.wav");

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
            string boundary = "----CustomBoundary" + DateTime.Now.Ticks.ToString("x");

            byte[] b1 = FileToByteArray(fileName);
            byte[] b2 = FileToByteArray2(fileName);

            Logger.Log("PostTest() - MultipartFormDataContent");
            using (var content = new MultipartFormDataContent(boundary))
            {
                Logger.Log("PostTest() - MultipartFormDataContent.Headers : " + content.Headers.Count().ToString() + " // "  + content.Headers.ToString());
                content.Headers.Remove("Content-Type");
                content.Headers.TryAddWithoutValidation("Content-Type", "multipart/form-data; boundary=" + boundary);
                Logger.Log("PostTest() - MultipartFormDataContent.Headers : " + content.Headers.Count().ToString() + " // " + content.Headers.ToString());

                content.Add(new StringContent("rotating_machine"), "machine_type");
                content.Add(new StringContent("My machine"), "machine_name");
                content.Add(new StringContent("My campaign"), "campaign_name");
                content.Add(new StringContent("Pierre-Christophe"), "user_name");
                content.Add(new ByteArrayContent(b1, 0, b1.Length), "audio_file", "audio.wav");

                Logger.Log("PostTest() - HttpClientHandler");
                HttpClientHandler handler = new HttpClientHandler();
                var cookieContainer = new CookieContainer();
                handler.CookieContainer = cookieContainer;

                Logger.Log("PostTest() - HttpRequestMessage");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://51.254.131.100/api/recordings/");
                request.Headers.ExpectContinue = false;
                request.Content = content;

                try
                {
                    Logger.Log("PostTest() - HttpClient");
                    var httpClient = new HttpClient(handler);
                    HttpResponseMessage response = await httpClient.SendAsync(request);
                    Logger.Log("PostTest() - response : " + response.ToString());
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

        async Task Post3(string fileName)
        {
            Logger.Log("Post3()");
            using (var client = new HttpClient())
            {
                using (var content = new MultipartFormDataContent())
                {
                    Logger.Log("Post3() - KeyValuePair");
                    var values = new []
                    {
                        new KeyValuePair<string, string>("rotating_machine", "machine_type"),
                        new KeyValuePair<string, string>("My machine", "machine_name"),                    
                        new KeyValuePair<string, string>("My campaign", "campaign"),
                        new KeyValuePair<string, string>("Pierre-Christophe", "user_name"),                  
                    };

                    foreach (var keyValuePair in values)
                    {
                        content.Add(new StringContent(keyValuePair.Value), keyValuePair.Key);
                    }

                    Logger.Log("Post3() - fileContent");
                    var fileContent = new ByteArrayContent(System.IO.File.ReadAllBytes(fileName));
                    fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = "audio.wav"
                    };
                    content.Add(fileContent);

                    try
                    {
                        Logger.Log("Post3() - PostAsync()");
                        var result = client.PostAsync(url, content).Result;
                        Logger.Log("Post3() - result : " + result.ToString());
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Post3() - Exception : " + ex.ToString());
                    }
                }
            }
        }

        private byte[] FileToByteArray(string fullFilePath)
        {
            Logger.Log("FileToByteArray()");
            FileStream fs = System.IO.File.OpenRead(fullFilePath);
            byte[] bytes = new byte[fs.Length];
            fs.Read(bytes, 0, Convert.ToInt32(fs.Length));
            fs.Close();
            Logger.Log("FileToByteArray() - bytes : " + bytes.Length.ToString());
            return bytes;
        }

        private byte[] FileToByteArray2(string fullFilePath)
        {
            Logger.Log("FileToByteArray2()");
            byte[] bytes = System.IO.File.ReadAllBytes(fullFilePath);
            Logger.Log("FileToByteArray2() - bytes : " + bytes.Length.ToString());
            return bytes;
        }

    }
}