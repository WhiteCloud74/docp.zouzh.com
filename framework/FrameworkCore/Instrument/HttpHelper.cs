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
            catch (Exception)
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
    }
}
