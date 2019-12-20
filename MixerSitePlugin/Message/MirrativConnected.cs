using SitePlugin;
using System.Collections.Generic;

namespace MixerSitePlugin
{
    internal class MixerConnected : MessageBase, IMixerConnected
    {
        public override SiteType SiteType { get; } = SiteType.Mixer;
        public MixerMessageType MixerMessageType { get; } = MixerMessageType.Connected;

        public MixerConnected(string raw) : base(raw)
        {
            CommentItems = new List<IMessagePart>
            {
                Common.MessagePartFactory.CreateMessageText("配信に接続しました"),
            };
        }
    }
}
