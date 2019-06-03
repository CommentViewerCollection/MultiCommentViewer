using System.Xml;

namespace NicoSitePlugin
{

    /// <summary>
    /// 
    /// </summary>
    public class Thread : IThread
    {
        //<thread resultcode="0" thread="1389881167" last_res="6006" ticket="0x32b22900" revision="1" server_time="1414839575"/>
        /// <summary>
        /// 
        /// </summary>
        /// 0:問題なし
        /// 1:失敗
        /// 3:threadにversionを入れずにサーバに送ったら帰ってきた。
        public int? Resultcode { get; internal set; }
        public string ThreadId { get; internal set; }
        public int? LastRes { get; internal set; }
        public string Ticket { get; internal set; }
        public int? Revision { get; internal set; }
        public long? ServerTime { get; internal set; }
        public string Raw { get; internal set; }
        public Thread() { }
        public Thread(string strThread)
        {
            Raw = strThread;
            using (var xmlReader = XmlReader.Create(new System.IO.StringReader($"<root>{strThread}</root>")))
            {
                xmlReader.ReadToFollowing("thread");
                if (int.TryParse(xmlReader.GetAttribute("resultcode"), out int resultcode_))
                    Resultcode = resultcode_;
                ThreadId = xmlReader.GetAttribute("thread");
                if (int.TryParse(xmlReader.GetAttribute("last_res"), out int last_res_))
                    LastRes = last_res_;
                Ticket = xmlReader.GetAttribute("ticket");
                if (int.TryParse(xmlReader.GetAttribute("revision"), out int revision_))
                    Revision = revision_;
                if (long.TryParse(xmlReader.GetAttribute("server_time"), out long server_time_))
                    ServerTime = server_time_;
            }
        }
    }
}
