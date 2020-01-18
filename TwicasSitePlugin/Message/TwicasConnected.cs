using SitePlugin;
using System.Collections.Generic;

namespace TwicasSitePlugin
{
    internal class TwicasConnected : MessageBase2, ITwicasConnected
    {
        public override SiteType SiteType { get; } = SiteType.Twicas;
        public TwicasMessageType TwicasMessageType { get; } = TwicasMessageType.Connected;
        public string Text { get; }

        public TwicasConnected(string raw) : base(raw)
        {
            Text = "接続しました";
        }
    }
}
