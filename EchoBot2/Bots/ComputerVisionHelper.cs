using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace EchoBot2.Bots
{


    public class ComputerVisionHelper
    {

        public static dynamic MakeRequest(string endpoint, string subscriptionKey, byte[] byteData)
        {
            HttpClient client = new HttpClient();
            string uriBase = endpoint + "vision/v2.1/analyze";

            // Request headers.
            client.DefaultRequestHeaders.Add(
                "Ocp-Apim-Subscription-Key", subscriptionKey);

            string requestParameters =
                "visualFeatures=Categories,Description,Color";

            // Assemble the URI for the REST API method.
            string uri = uriBase + "?" + requestParameters;

            HttpResponseMessage response;

            // Add the byte array as an octet stream to the request body.
            using (ByteArrayContent content = new ByteArrayContent(byteData))
            {
                // This example uses the "application/octet-stream" content type.
                // The other content types you can use are "application/json"
                // and "multipart/form-data".
                content.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                // Asynchronously call the REST API method.
                response = client.PostAsync(uri, content).Result;
            }

            // Asynchronously get the JSON response.
            string JSON = response.Content.ReadAsStringAsync().Result;

            return Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(JSON);
        }

        /// <summary>
        /// 處理照片
        /// </summary>
        /// <param name="LineEvent"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static dynamic ProcessImage(string endpoint, string subscriptionKey, byte[] byteData)
        {
            string Msg = "";
            string analyze = "";

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            //取得圖片檔案FileStream, 分別作為繪圖與分析用
            Stream MemStream1 = new MemoryStream(byteData);
            Stream MemStream2 = new MemoryStream(byteData);
            //繪圖用
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(MemStream1);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp);
            //ComputerVision instance
            var visionClient = new Microsoft.Azure.CognitiveServices.Vision.ComputerVision.ComputerVisionClient(
                 new Microsoft.Azure.CognitiveServices.Vision.ComputerVision.ApiKeyServiceClientCredentials(subscriptionKey))
            { Endpoint = endpoint };

            //分析用
            using (MemStream2)
            {
                var features = new List<VisualFeatureTypes?>() { VisualFeatureTypes.Faces, VisualFeatureTypes.Description };
                //分析圖片
                var Results = visionClient.AnalyzeImageInStreamAsync(
                    MemStream2, features).Result;
                //分別保存性別數量
                int isM = 0, isF = 0;
                //如果找到臉，就畫方框標示出來
                foreach (var item in Results.Faces)
                {
                    var faceRect = item.FaceRectangle;
                    //畫框
                    g.DrawRectangle(
                                new Pen(Brushes.Red, 3),
                                new Rectangle(faceRect.Left, faceRect.Top,
                                    faceRect.Width, faceRect.Height));
                    //在方框旁邊顯示年紀
                    var age = 0;
                    if (item.Gender == Gender.Female) age = item.Age - 2; else age = item.Age;
                    //劃出數字
                    g.DrawString(age.ToString(), new Font(SystemFonts.DefaultFont.FontFamily, 24, FontStyle.Bold),
                        new SolidBrush(Color.Black),
                        faceRect.Left + 3, faceRect.Top + 3);
                    //紀錄性別數量
                    if (item.Gender == Gender.Male)
                        isM += 1;
                    else
                        isF += 1;
                }
                //圖片分析結果
                Msg += $"你上傳的圖片，經過辨識是：";
                try
                {
                    analyze = Results.Description.Captions[0].Text;
                    Msg += $"\n{ analyze}";
                }
                catch (Exception ex)
                {
                    Msg += $"\n{ Results.Description.Captions[0].Text}";
                }


                //如果update了照片，則顯示新圖
                if (Results.Faces.Count() > 0)
                {
                    Msg += String.Format("\n\n找到{0}張臉, \n{1}男 {2}女", Results.Faces.Count(), isM, isF);
                }
                else
                {
                    Msg += "\n\n這照片裡面沒有人耶...";
                }
            }

            string ImgurURL = "";
            using (MemoryStream m = new MemoryStream())
            {
                bmp.Save(m, System.Drawing.Imaging.ImageFormat.Png);
                ImgurURL = UploadImage2Imgur(m.ToArray());
            }

            dynamic ret = new { Text = Msg, ImageURL = new Uri(ImgurURL) };
            //一次把集合中的多則訊息回覆給用戶
            return ret;
        }

        //Upload Image to Imgur
        private static string UploadImage2Imgur(byte[] bytes)
        {
            var client = new HttpClient();
            var clientId = "👉__ImgurClientId_____"; //like 16axx15eb890ff4
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Client-ID", clientId);

            var content = new MultipartFormDataContent();
            content.Add(new ByteArrayContent(bytes), "image");

            var response = client.PostAsync("https://api.imgur.com/3/image", content).Result;
            if (response.IsSuccessStatusCode)
            {
                var data = JsonConvert.DeserializeObject<dynamic>( response.Content.ReadAsStringAsync().Result);
                Console.WriteLine($"圖片上傳成功, 圖片網址: {data.data.link}");
                return data.data.link;
            }
            else
            {
                Console.WriteLine("圖片上傳失敗");
                return null;
            }
        }

    }

}
