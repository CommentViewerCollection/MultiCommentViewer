using SitePlugin;
using System.Collections.Generic;

namespace ShowRoomSitePlugin
{
    internal class ShowRoomConnected : MessageBase2, IShowRoomConnected
    {
        public override SiteType SiteType { get; } = SiteType.ShowRoom;
        public ShowRoomMessageType ShowRoomMessageType { get; } = ShowRoomMessageType.Connected;
        public string Text { get; }

        public ShowRoomConnected(string raw) : base(raw)
        {
            Text = "接続しました";
        }
    }
}
