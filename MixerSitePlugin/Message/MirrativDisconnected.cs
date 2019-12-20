using SitePlugin;
using System.Collections.Generic;

namespace MixerSitePlugin
{
    internal class MixerDisconnected : MessageBase, IMixerDisconnected
    {
        public override SiteType SiteType { get; } = SiteType.Mixer;
        public MixerMessageType MixerMessageType { get; } = MixerMessageType.Disconnected;
        public MixerDisconnected(string raw) : base(raw)
        {
            CommentItems = new List<IMessagePart>
            {
                Common.MessagePartFactory.CreateMessageText("配信が終了しました"),
            };
        }
    }
}
