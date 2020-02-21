using SitePlugin;
using System.Collections.Generic;

namespace ShowRoomSitePlugin
{
    internal class ShowRoomDisconnected : MessageBase2, IShowRoomDisconnected
    {
        public override SiteType SiteType { get; } = SiteType.ShowRoom;
        public ShowRoomMessageType ShowRoomMessageType { get; } = ShowRoomMessageType.Disconnected;
        public string Text { get; }

        public ShowRoomDisconnected(string raw) : base(raw)
        {
            Text = "切断しました";
        }
    }
}
