using Newtonsoft.Json;

namespace NicoSitePlugin.Chat
{
    class Ping : IChatMessage
    {
        public string Content { get; }
        public string Raw { get; }

        public Ping(string raw)
        {
            dynamic d = JsonConvert.DeserializeObject(raw);
            Content = (string)d.ping.content;
            Raw = raw;
        }
    }
}
