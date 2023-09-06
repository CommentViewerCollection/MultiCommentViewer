﻿using Mcv.PluginV2;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace MildomSitePlugin
{
    public class MildomServer : ServerBase, IDataServer
    {
        public async Task<string> GetAsync(string url, Dictionary<string, string> headers)
        {
            var result = await GetInternalAsync(new HttpOptions
            {
                Url = url,
                Headers = headers,
            });
            var str = await result.Content.ReadAsStringAsync();
            return str;
        }
        public async Task<string> GetAsync(string url, Dictionary<string, string> headers, CookieContainer cc)
        {
            var result = await GetInternalAsync(new HttpOptions
            {
                Url = url,
                Cc = cc,
                Headers = headers,
            });
            var str = await result.Content.ReadAsStringAsync();
            return str;
        }
        public async Task<string> GetAsync(string url)
        {
            var result = await GetInternalAsync(new HttpOptions
            {
                Url = url,
            });
            var str = await result.Content.ReadAsStringAsync();
            return str;
        }
        public async Task<string> PostAsync(string url, Dictionary<string, string> data, CookieContainer cc)
        {
            var content = new FormUrlEncodedContent(data);
            var result = await PostInternalAsync(new HttpOptions
            {
                Url = url,
                Cc = cc,
            }, content);
            var str = await result.Content.ReadAsStringAsync();
            return str;
        }
    }
}
