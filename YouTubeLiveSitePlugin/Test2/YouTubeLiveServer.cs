using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Net.Http;
using System;
using System.Net.Http.Headers;

namespace YouTubeLiveSitePlugin.Test2
{
    public class HttpOptions
    {
        public string Url { get; set; }
        public string UserAgent { get; set; }
        public CookieContainer Cc { get; set; }
        public List<StringWithQualityHeaderValue> AcceptLanguages { get; set; }
    }
    public class YouTubeLiveServer : IYouTubeLibeServer
    {
        public async Task<string> GetAsync(HttpOptions options)
        {
            if (string.IsNullOrEmpty(options.Url))
            {
                throw new ArgumentNullException(nameof(options));
            }

            var handler = new HttpClientHandler();
            if (options.Cc != null)
            {
                handler.UseCookies = true;
                handler.CookieContainer = options.Cc;
            }
            using (var client = new HttpClient(handler))
            {
                if (!string.IsNullOrEmpty(options.UserAgent))
                {
                    client.DefaultRequestHeaders.Add("User-Agent", options.UserAgent);
                }
                if (options.AcceptLanguages != null)
                {
                    foreach (var la in options.AcceptLanguages)
                    {
                        client.DefaultRequestHeaders.AcceptLanguage.Add(la);
                    }
                }
                var ret = await client.GetAsync(options.Url);
                if (!ret.IsSuccessStatusCode)
                {
                    var errorRes = await ret.Content.ReadAsStringAsync();
                    throw new HttpException
                    {
                        StatusCode = (int)ret.StatusCode,
                        Response = errorRes,
                    };
                }
                return await ret.Content.ReadAsStringAsync();
            }
        }
        public Task<string> GetAsync(string url, CookieContainer cc)
        {
            var options = new HttpOptions
            {
                Url = url,
                Cc = cc,
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:59.0) Gecko/20100101 Firefox/59.0",
            };
            return GetAsync(options);
        }
        public Task<string> GetAsync(string url)
        {
            var options = new HttpOptions
            {
                Url = url,
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:59.0) Gecko/20100101 Firefox/59.0",
            };
            return GetAsync(options);
        }

        public Task<string> GetEnAsync(string url)
        {
            var options = new HttpOptions
            {
                Url = url,
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:59.0) Gecko/20100101 Firefox/59.0",
                AcceptLanguages = new List<StringWithQualityHeaderValue>
                {
                    new StringWithQualityHeaderValue("en-US"),
                    new StringWithQualityHeaderValue("en", 0.5),
                },
            };
            return GetAsync(options);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="cc"></param>
        /// <returns></returns>
        /// <exception cref="TaskCanceledException"></exception>
        public async Task<string> PostAsync(string url, Dictionary<string, string> data, CookieContainer cc)
        {
            var userAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.2924.87 Safari/537.36";
            var content = new FormUrlEncodedContent(data);
            using (var handler = new HttpClientHandler { UseCookies = true, CookieContainer = cc })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", userAgent);
                var result = await client.PostAsync(url, content);
                var resBody = await result.Content.ReadAsStringAsync();
                if (result.IsSuccessStatusCode)
                {
                    return resBody;
                }
                else
                {
                    throw new HttpException
                    {
                        StatusCode = (int)result.StatusCode,
                        Response = resBody,
                    };
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">options.Urlがnullの時</exception>
        /// <exception cref="TaskCanceledException"></exception>
        public async Task<string> PostAsync(HttpOptions options, HttpContent content)
        {
            var result = await PostInternalAsync(options, content);
            return await result.Content.ReadAsStringAsync();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        private async Task<HttpResponseMessage> PostInternalAsync(HttpOptions options, HttpContent content)
        {
            if (string.IsNullOrEmpty(options.Url))
            {
                throw new ArgumentNullException(nameof(options));
            }

            var handler = new HttpClientHandler();
            if (options.Cc != null)
            {
                handler.UseCookies = true;
                handler.CookieContainer = options.Cc;
            }
            using (var client = new HttpClient(handler))
            {
                if (!string.IsNullOrEmpty(options.UserAgent))
                {
                    client.DefaultRequestHeaders.Add("User-Agent", options.UserAgent);
                }
                if (options.AcceptLanguages != null)
                {
                    foreach (var la in options.AcceptLanguages)
                    {
                        client.DefaultRequestHeaders.AcceptLanguage.Add(la);
                    }
                }

                var result = await client.PostAsync(options.Url, content);
                if (!result.IsSuccessStatusCode)
                {
                    throw new HttpException
                    {
                        StatusCode = (int)result.StatusCode,
                        Response = await result.Content.ReadAsStringAsync(),
                    };
                }
                return result;
            }
        }
        public async Task<byte[]> GetBytesAsync(string url)
        {
            var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:59.0) Gecko/20100101 Firefox/59.0";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", userAgent);
                var t = await client.GetByteArrayAsync(url);
                //ここでawaitせずにTaskをreturnしようとすると、先にDispose()が実行されるためTaskがCancelされてしまう。
                return t;
            }
        }
    }
}
