using System.Xml.Serialization;

namespace NicoSitePlugin.Old
{
    class HeartbeatResponse
    {
        public bool Success => Heartbeat != null;
        public IHeartbeat Heartbeat { get; }
        public IHeartbeartFail Fail { get; }
        public HeartbeatResponse(IHeartbeat heartbeat)
        {
            Heartbeat = heartbeat;
        }
        public HeartbeatResponse(IHeartbeartFail fail)
        {
            Fail = fail;
        }
    }
    public interface IHeartbeat
    {
        string CommentCount { get; }
        string Isrestrict { get; }
        string Status { get; }
        string Ticket { get; }
        string Time { get; }
        string WaitTime { get; }
        string WatchCount { get; }
    }
    public interface IHeartbeartFail
    {
        string Code { get; }
        string Description { get; }
        string Reject { get; }
    }

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
    [XmlRoot(ElementName = "error")]
    public class HeartbeartError
    {
        [XmlElement(ElementName = "code")]
        public string Code { get; set; }
        [XmlElement(ElementName = "description")]
        public string Description { get; set; }
        [XmlElement(ElementName = "reject")]
        public string Reject { get; set; }
    }
}