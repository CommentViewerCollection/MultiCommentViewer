using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Net.Http;
using System;
using System.Net.Http.Headers;
using SitePluginCommon;

namespace YouTubeLiveSitePlugin.Test2
{
    public class YouTubeLiveServer : ServerBase, IYouTubeLiveServer
    {
        public async Task<string> GetAsync(HttpOptions options)
        {
            var ret = await GetInternalAsync(options);
            return await ret.Content.ReadAsStringAsync();

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
            var options = new HttpOptions
            {
                Url = url,
                UserAgent = userAgent,
                Cc = cc,
            };
            var content = new FormUrlEncodedContent(data);
            var ret = await PostAsync(options, content);
            return ret;
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
        public async Task<byte[]> GetBytesAsync(string url)
        {
            var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:59.0) Gecko/20100101 Firefox/59.0";
            var options = new HttpOptions
            {
                Url = url,
                UserAgent = userAgent,
            };
            var message = await GetInternalAsync(options);
            return await message.Content.ReadAsByteArrayAsync();
        }

        public Task<HttpResponseMessage> PostJsonNoThrowAsync(string url, Dictionary<string, string> headers, string payload, CookieContainer cc)
        {
            var options = new HttpOptions
            {
                Url = url,
                Cc = cc,
                Headers = headers,
            };
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            return PostInternalNoThrowAsync(options, content);
        }
    }
}
