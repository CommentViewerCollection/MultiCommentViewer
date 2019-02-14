using System;
using System.Text.RegularExpressions;

namespace NicoSitePlugin
{
    class Room : IXmlWsRoomInfo
    {
        public Room() { }
        public Room(Low.ProgramInfo.Room low)
        {
            if (low == null) throw new ArgumentNullException(nameof(low));
            var match = Regex.Match(low.XmlSocketUri, "^[^:/]+://([^:]+):(\\d+)$");
            if (!match.Success) throw new ParseException(low.XmlSocketUri);
            XmlSocketAddr = match.Groups[1].Value;
            XmlSocketPort = int.Parse(match.Groups[2].Value);
            WebSocketUri = low.WebSocketUri;
            Name = low.Name;
            Id = low.Id;
            ThreadId = low.ThreadId;
        }

        public string XmlSocketAddr { get; set; }

        public int XmlSocketPort { get; set; }

        public string WebSocketUri { get; set; }

        public string Name { get; set; }

        public long Id { get; set; }

        public string ThreadId { get; set; }

        string IRoomInfo.Addr => XmlSocketAddr;

        int IRoomInfo.Port => XmlSocketPort;

        string IRoomInfo.Thread => ThreadId;

        string IRoomInfo.RoomLabel => Name;

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
            {
                return true;
            }
            var c2 = obj as Room;
            if ((object)c2 == null)
                return false;
            var c1 = this;
            return (c1.XmlSocketAddr == c2.XmlSocketAddr) && (c1.XmlSocketPort == c2.XmlSocketPort) && (c1.ThreadId == c2.ThreadId);
        }
        public override int GetHashCode()
        {
            return this.XmlSocketAddr.GetHashCode() ^ this.XmlSocketPort.GetHashCode() ^ this.ThreadId.GetHashCode();
        }
    }
}
