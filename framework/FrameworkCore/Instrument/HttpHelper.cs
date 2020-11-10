using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace FrameworkCore.Instrument
{
    public static class HttpHelper
    {
        #region HttpWebRequest
        //POST方法
        public static string HttpWebRequestPost(string Url, string postDataStr)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            Encoding encoding = Encoding.UTF8;
            byte[] postData = encoding.GetBytes(postDataStr);
            request.ContentLength = postData.Length;
            Stream myRequestStream = request.GetRequestStream();
            myRequestStream.Write(postData, 0, postData.Length);
            myRequestStream.Close();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, encoding);
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }

        //GET方法
        public static string HttpWebRequestGet(string Url, string postDataStr)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url + (postDataStr == "" ? "" : "?") + postDataStr);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();
            return retString;
        }
        #endregion HttpWebRequest

        #region WebClient
        public static string WebClientDownloadString(string url)
        {
            WebClient wc = new WebClient
            {
                //wc.BaseAddress = url;   //设置根目录
                Encoding = Encoding.UTF8    //设置按照何种编码访问，如果不加此行，获取到的字符串中文将是乱码
            };
            string str = wc.DownloadString(url);
            return str;
        }
        public static string WebClientDownloadStreamString(string url)
        {
            WebClient wc = new WebClient();
            wc.Headers.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/76.0.3809.132 Safari/537.36");
            Stream objStream = wc.OpenRead(url);
            StreamReader _read = new StreamReader(objStream, Encoding.UTF8);    //新建一个读取流，用指定的编码读取，此处是utf-8
            string str = _read.ReadToEnd();
            objStream.Close();
            _read.Close();
            return str;
        }

        public static void WebClientDownloadFile(string url, string filename)
        {
            WebClient wc = new WebClient();
            wc.DownloadFile(url, filename);     //下载文件
        }

        public static void WebClientDownloadData(string url, string filename)
        {
            WebClient wc = new WebClient();
            byte[] bytes = wc.DownloadData(url);   //下载到字节数组
            FileStream fs = new FileStream(filename, FileMode.Create);
            fs.Write(bytes, 0, bytes.Length);
            fs.Flush();
            fs.Close();
        }

        public static void WebClientDownloadFileAsync(string url, string filename)
        {
            WebClient wc = new WebClient();
            wc.DownloadFileCompleted += WebClientDownCompletedEventHandler;
            wc.DownloadFileAsync(new Uri(url), filename);
            Console.WriteLine("下载中。。。");
        }
        private static void WebClientDownCompletedEventHandler(object sender, AsyncCompletedEventArgs e)
        {
            Console.WriteLine(sender.ToString());   //触发事件的对象
            Console.WriteLine(e.UserState);
            Console.WriteLine(e.Cancelled);
            Console.WriteLine("异步下载完成！");
        }

        public static void WebClientDownloadFileAsync2(string url, string filename)
        {
            WebClient wc = new WebClient();
            wc.DownloadFileCompleted += (sender, e) =>
            {
                Console.WriteLine("下载完成!");
                Console.WriteLine(sender.ToString());
                Console.WriteLine(e.UserState);
                Console.WriteLine(e.Cancelled);
            };
            wc.DownloadFileAsync(new Uri(url), filename);
            Console.WriteLine("下载中。。。");
        }
        #endregion WebClient

        #region HttpClient

        private static readonly HttpClient client;
        static HttpHelper()
        {
            var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.None };
            client = new HttpClient(handler);
        }
        public static async Task<string> HttpClientPostAsync(string url, List<KeyValuePair<string, string>> paramArray)
        {
            return await HttpClientPostAsync(url, BuildParam(paramArray));
        }

        public static async Task<string> HttpClientPostAsync(string url, string postData)
        {
            try
            {
                HttpContent content = new StringContent(postData);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage res = await client.PostAsync(url, content);

                return res.StatusCode == HttpStatusCode.OK ? await res.Content.ReadAsStringAsync() : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string HttpClientPost(string url, List<KeyValuePair<string, string>> paramArray)
        {
            return HttpClientPost(url, BuildParam(paramArray));
        }

        public static string HttpClientPost(string url, string postData)
        {
            try
            {
                HttpContent content = new StringContent(postData);

                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage res = client.PostAsync(url, content).Result;

                return res.StatusCode == HttpStatusCode.OK ? res.Content.ReadAsStringAsync().Result : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task<string> HttpClientGetAsync(string url, List<KeyValuePair<string, string>> paramArray)
        {
            return await HttpClientGetAsync(url + "?" + BuildParam(paramArray));
        }

        public static async Task<string> HttpClientGetAsync(string url)
        {
            try
            {
                return await client.GetStringAsync(url);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string HttpClientGet(string url, List<KeyValuePair<string, string>> paramArray)
        {
            return HttpClientGet(url + "?" + BuildParam(paramArray));
        }

        public static string HttpClientGet(string url)
        {
            try
            {
                return client.GetStringAsync(url).Result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string HttpClienPut(string url, string putData)
        {
            try
            {
                HttpContent content = new StringContent(putData);

                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage res = client.PutAsync(url, content).Result;

                return res.StatusCode == HttpStatusCode.OK ? res.Content.ReadAsStringAsync().Result : null;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static async Task<string> HttpClienPutAsync(string url, string putData)
        {
            try
            {
                HttpContent content = new StringContent(putData);

                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage res = await client.PutAsync(url, content);

                return res.StatusCode == HttpStatusCode.OK ? await res.Content.ReadAsStringAsync() : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string BuildParam(List<KeyValuePair<string, string>> paramArray, Encoding encode = null)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var item in paramArray)
            {
                sb.Append($"{ Encode(item.Key, encode)}={ Encode(item.Value, encode)}&");
            }

            return sb.ToString().TrimEnd('&');

            static string Encode(string content, Encoding encode = null)
            {
                return HttpUtility.UrlEncode(content, encode ?? Encoding.UTF8);
            }
        }

        /// <summary>
        /// get请求，可以对请求头进行多项设置
        /// </summary>
        /// <param name="paramArray"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetResponseByGet(List<KeyValuePair<string, string>> paramArray, string url)
        {
            string result = "";

            url = url + "?" + BuildParam(paramArray);
            var response = client.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                Stream myResponseStream = response.Content.ReadAsStreamAsync().Result;
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                result = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
            }

            return result;
        }
        public static string GetResponseBySimpleGet(List<KeyValuePair<string, string>> paramArray, string url)
        {
            url = url + "?" + BuildParam(paramArray);
            var result = client.GetStringAsync(url).Result;
            return result;
        }

        public static string HttpPostRequestAsync(string Url, List<KeyValuePair<string, string>> paramArray,
            string ContentType = "application/x-www-form-urlencoded")
        {
            string result = "";

            var postData = BuildParam(paramArray);

            var data = Encoding.ASCII.GetBytes(postData);

            try
            {
                client.DefaultRequestHeaders.Add("User-Agent",
                        @"Mozilla/5.0 (compatible; Baiduspider/2.0; +http://www.baidu.com/search/spider.html)");
                client.DefaultRequestHeaders.Add("Accept",
                        @"text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");

                HttpResponseMessage message = null;
                using (Stream dataStream = new MemoryStream(data ?? new byte[0]))
                {
                    using HttpContent content = new StreamContent(dataStream);
                    content.Headers.Add("Content-Type", ContentType);
                    var task = client.PostAsync(Url, content);
                    message = task.Result;
                }
                if (message != null && message.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    using (message)
                    {
                        result = message.Content.ReadAsStringAsync().Result;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return result;
        }
        #endregion HttpClient

    }
}
