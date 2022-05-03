using Newtonsoft.Json;

namespace NicoSitePlugin.Chat
{
    class ChatMessage : IChatMessage
    {
        public string Thread { get; }
        public int? No { get; }
        public long? Vpos { get; }
        public long Date { get; }
        public long DateUsec { get; }
        public string Mail { get; }
        public string UserId { get; }
        public int Anonymity { get; }
        public int Premium { get; }
        public string Content { get; }
        public string Raw { get; }
        public ChatMessage(string json)
        {
            dynamic d = JsonConvert.DeserializeObject(json);
            Thread = (string)d.chat.thread;
            if (d.chat.ContainsKey("no"))
            {
                No = (int)d.chat.no;
            }
            Vpos = (long?)d.chat.vpos;
            Date = (long)d.chat.date;
            DateUsec = (long)d.chat.date_usec;
            Mail = (string)d.chat.mail;
            UserId = (string)d.chat.user_id;
            if (d.chat.ContainsKey("anonymity"))
            {
                Anonymity = (int)d.chat.anonymity;
            }
            if (d.chat.ContainsKey("premium"))
            {
                Premium = (int)d.chat.premium;
            }

            Content = (string)d.chat.content;
        }
    }
}
