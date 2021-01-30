using System;

namespace NicoSitePlugin.Metadata
{
    internal class ServerTime : IMetaMessage
    {
        
        public DateTime CurrentMs { get; }
        public ServerTime(string raw)
        {
            //{"type":"serverTime","data":{"currentMs":"2021-01-29T23:59:09.461+09:00"}}
            Raw = raw;
            dynamic d = Newtonsoft.Json.JsonConvert.DeserializeObject(raw);
            CurrentMs = DateTime.Parse((string)d.data.currentMs);
        }

        public string Raw { get; }
    }
}