using System.Xml.Serialization;

namespace NicoSitePlugin.Low.Heartbeat
{
    [XmlRoot(ElementName = "heartbeat")]
    public class RootObject : IHeartbeat, IHeartbeartFail
    {
        [XmlElement(ElementName = "error")]
        public Low.HeartbeartError.RootObject Error { get; set; }
        [XmlElement(ElementName = "watchCount")]
        public string WatchCount { get; set; }
        [XmlElement(ElementName = "commentCount")]
        public string CommentCount { get; set; }
        [XmlElement(ElementName = "is_restrict")]
        public string Isrestrict { get; set; }
        [XmlElement(ElementName = "ticket")]
        public string Ticket { get; set; }
        [XmlElement(ElementName = "waitTime")]
        public string WaitTime { get; set; }
        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; }
        [XmlAttribute(AttributeName = "time")]
        public string Time { get; set; }

        public string Code => Error?.Code;
        public string Description => Error?.Description;
        public string Reject => Error?.Reject;

    }
}
