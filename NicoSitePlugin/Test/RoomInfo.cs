namespace NicoSitePlugin.Old
{
    public class RoomInfo
    {
        public string Thread { get; }
        public string Addr { get; }
        public int Port { get; }
        public string RoomLabel { get; }
        public RoomInfo(IMs ms, string roomLabel)
        {
            Thread = ms.Thread;
            Addr = ms.Addr;
            Port = ms.Port;
            RoomLabel = roomLabel;
        }
        
        public static bool operator ==(RoomInfo c1, RoomInfo c2)
        {
            if (object.ReferenceEquals(c1, c2))
            {
                return true;
            }
            if (((object)c1 == null) || ((object)c2 == null))
            {
                return false;
            }

            return (c1.Addr == c2.Addr) && (c1.Port == c2.Port) && (c1.Thread == c2.Thread);
        }
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
            {
                return true;
            }
            var c2 = obj as RoomInfo;
            if (c2 == null)
                return false;
            var c1 = this;
            return (c1.Addr == c2.Addr) && (c1.Port == c2.Port) && (c1.Thread == c2.Thread);
        }

        public static bool operator !=(RoomInfo c1, RoomInfo c2)
        {
            return !(c1 == c2);
        }
    }
}
