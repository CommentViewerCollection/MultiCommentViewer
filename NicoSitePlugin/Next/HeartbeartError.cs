using System.Xml.Serialization;

namespace NicoSitePlugin.Next
{
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
