using System.Xml.Serialization;

namespace NicoSitePlugin.Next
{
    [XmlRoot(ElementName = "heartbeat")]
    public class Heartbeat : IHeartbeat, IHeartbeartFail
    {
        [XmlElement(ElementName = "error")]
        public HeartbeartError Error { get; set; }
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
