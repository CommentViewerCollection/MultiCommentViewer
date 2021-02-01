using Newtonsoft.Json;

namespace NicoSitePlugin.Metadata
{
    class PostCommentResult : IMetaMessage
    {
        public PostCommentResult(string raw)
        {
            dynamic d = JsonConvert.DeserializeObject(raw);
            Content = (string)d.data.chat.content;
            Mail = (string)d.data.chat.mail;
            Anonymity = (int)d.data.chat.anonymity;
            Restricted = (bool)d.data.chat.restricted;
        }
        public string Content { get; }
        public string Mail { get; }
        public int Anonymity { get; }
        public bool Restricted { get; }
        public string Raw
        {
            get
            {
                return $"{{\"type\":\"postCommentResult\",\"data\":{{\"chat\":{{\"content\":\"{Content}\",\"mail\":\"{Mail}\",\"anonymity\":{Anonymity},\"restricted\":{Restricted.ToString().ToLower()}}}}}}}";
            }
        }
    }
    class PostComment : IMetaMessage
    {
        public string Raw
        {
            get
            {
                var data = $"\"text\":\"{Text}\",\"vpos\":{Vpos},\"isAnonymous\":{IsAnonymous.ToString().ToLower()}";
                if (Color != null)
                {
                    data += $",\"color\":\"{Color}\"";
                }
                if (Size != null)
                {
                    data += $",\"size\":\"{Size}\"";
                }
                if (Position != null)
                {
                    data += $",\"position\":\"{Position}\"";
                }
                var s = $"{{\"type\":\"postComment\",\"data\":{{{data}}}}}";
                return s;
            }
        }

        public string Text { get; }
        public long Vpos { get; }
        public bool IsAnonymous { get; }
        public string Color { get; }
        public string Size { get; }
        public string Position { get; }

        public PostComment(string text, long vpos, bool isAnonymous, string color = null, string size = null, string position = null)
        {
            Text = text;
            Vpos = vpos;
            IsAnonymous = isAnonymous;
            Color = color;
            Size = size;
            Position = position;
        }
    }
}