﻿using System.Threading.Tasks;
using System.Net;
using System.Collections.Generic;
using System.Net.Http;
using SitePluginCommon;

namespace BigoSitePlugin
{
    public interface IBigoServer
    {
        Task<string> GetAsync(string url);
        Task<string> GetAsync(string url, CookieContainer cc);
        Task<string> GetAsync(string url, Dictionary<string, string> headers, CookieContainer cc);
        Task<string> GetEnAsync(string url);
        Task<string> PostAsync(string url, Dictionary<string, string> data, CookieContainer cc);
        Task<string> PostAsync(HttpOptions options, HttpContent content);
        Task<byte[]> GetBytesAsync(string url);
    }
}
