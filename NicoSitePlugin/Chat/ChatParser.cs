using Newtonsoft.Json;

namespace NicoSitePlugin.Chat
{
    static class ChatParser
    {
        public static IChatMessage Parse(string raw)
        {
            IChatMessage message;
            dynamic d = JsonConvert.DeserializeObject(raw);
            if (d.ContainsKey("chat"))
            {
                message = new ChatMessage(raw);
            }
            else if (d.ContainsKey("thread"))
            {
                message = new Thread();
            }
            else if (d.ContainsKey("ping"))
            {
                message = new Ping(raw);
            }
            else
            {
                message = new UnknownMessage(raw);
            }
            return message;
        }
    }
}
