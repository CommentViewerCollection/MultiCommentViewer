using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SitePluginCommon
{
    public class HttpOptions
    {
        public string Url { get; set; }
        public string UserAgent { get; set; }
        public CookieContainer Cc { get; set; }
        public List<StringWithQualityHeaderValue> AcceptLanguages { get; set; }
        public Dictionary<string, string> Headers { get; set; }
    }
    public abstract class ServerBase
    {
        protected async Task<HttpResponseMessage> GetInternalAsync(HttpOptions options)
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
                if(options.Headers != null)
                {
                    foreach(var kv in options.Headers)
                    {
                        client.DefaultRequestHeaders.Add(kv.Key, kv.Value);
                    }
                }
                var ret = await client.GetAsync(options.Url);
                ret.EnsureSuccessStatusCode();
                return ret;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        protected async Task<HttpResponseMessage> PostInternalAsync(HttpOptions options, HttpContent content)
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
                if (options.Headers != null)
                {
                    foreach (var kv in options.Headers)
                    {
                        client.DefaultRequestHeaders.Add(kv.Key, kv.Value);
                    }
                }
                var result = await client.PostAsync(options.Url, content);
                result.EnsureSuccessStatusCode();
                return result;
            }
        }
    }
}
