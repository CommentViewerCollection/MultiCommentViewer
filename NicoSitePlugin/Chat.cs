using System;
using System.Xml;

namespace NicoSitePlugin
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class Chat : IChat
    {
        public string Thread { get; internal set; }
        public int? No { get; protected set; }
        private string _vpos_str;
        public long? Vpos
        {
            get
            {
                if (long.TryParse(_vpos_str, out long vpos))
                {
                    return vpos;
                }
                else
                    return null;
            }
        }
        public string DateStr { get; internal set; }
        public DateTime Date
        {
            get
            {
                return FromUnixTime(long.Parse(DateStr));
            }
        }
        private static DateTime FromUnixTime(long unix)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(unix).ToLocalTime();
        }
        public long? DateUsec
        {
            get
            {
                if (long.TryParse(_date_usec_str, out long _date_usec))
                {
                    return _date_usec;
                }
                else
                    return null;
            }
        }
        private string _date_usec_str;
        public string Mail { get; internal set; }
        public string UserId { get; internal set; }
        public int? Premium { get; internal set; }
        public int? Anonymity { get; internal set; }
        public string Locale { get; internal set; }
        public int? Score { get; internal set; }
        public string Text { get; internal set; }
        public bool Yourpost { get; internal set; }
        public string Origin { get; internal set; }
        public bool IsBsp { get { return Text.StartsWith("/press show "); } }
        public string Raw { get; internal set; }
        public Chat()
        {
        }
        public static Chat Parse(string strChat)
        {
            var chat = new Chat
            {
                Raw = strChat
            };
            using (var xmlReader = XmlReader.Create(new System.IO.StringReader($"<root>{strChat}</root>")))
            {
                xmlReader.ReadToFollowing("chat");
                if (int.TryParse(xmlReader.GetAttribute("no"), out int no_))
                    chat.No = no_;
                chat._vpos_str = xmlReader.GetAttribute("vpos");
                chat.Thread = xmlReader.GetAttribute("thread");
                chat.DateStr = xmlReader.GetAttribute("date");
                chat._date_usec_str = xmlReader.GetAttribute("date_usec");
                chat.Mail = xmlReader.GetAttribute("mail");
                chat.UserId = xmlReader.GetAttribute("user_id");
                if (int.TryParse(xmlReader.GetAttribute("premium"), out int premium_))
                    chat.Premium = premium_;
                if (int.TryParse(xmlReader.GetAttribute("anonymity"), out int anonymity_))
                    chat.Anonymity = anonymity_;
                chat.Locale = xmlReader.GetAttribute("locale");
                if (int.TryParse(xmlReader.GetAttribute("score"), out int score_))
                    chat.Score = score_;
                chat.Yourpost = !string.IsNullOrWhiteSpace(xmlReader.GetAttribute("yourpost"));
                chat.Origin = xmlReader.GetAttribute("origin");
                //以前はHtmlConverter.Decode()をしていたけど、ここでは一切加工しないことに。
                chat.Text = xmlReader.ReadElementContentAsString();
            }
            return chat;
        }
        public Chat(string strChat)
        {
            Raw = strChat;
            using (var xmlReader = XmlReader.Create(new System.IO.StringReader($"<root>{strChat}</root>")))
            {
                xmlReader.ReadToFollowing("chat");
                if (int.TryParse(xmlReader.GetAttribute("no"), out int no_))
                    No = no_;
                _vpos_str = xmlReader.GetAttribute("vpos");
                Thread = xmlReader.GetAttribute("thread");
                DateStr = xmlReader.GetAttribute("date");
                _date_usec_str = xmlReader.GetAttribute("date_usec");
                Mail = xmlReader.GetAttribute("mail");
                UserId = xmlReader.GetAttribute("user_id");
                if (int.TryParse(xmlReader.GetAttribute("premium"), out int premium_))
                    Premium = premium_;
                if (int.TryParse(xmlReader.GetAttribute("anonymity"), out int anonymity_))
                    Anonymity = anonymity_;
                Locale = xmlReader.GetAttribute("locale");
                if (int.TryParse(xmlReader.GetAttribute("score"), out int score_))
                    Score = score_;
                Yourpost = !string.IsNullOrWhiteSpace(xmlReader.GetAttribute("yourpost"));
                Origin = xmlReader.GetAttribute("origin");
                //以前はHtmlConverter.Decode()をしていたけど、ここでは一切加工しないことに。
                Text = xmlReader.ReadElementContentAsString();
            }
        }
    }
}
