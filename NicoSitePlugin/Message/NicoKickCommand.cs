using SitePlugin;

namespace NicoSitePlugin
{
    internal class NicoKickCommand : MessageBase2, INicoKickCommand
    {
        public override SiteType SiteType { get; } = SiteType.NicoLive;
        public NicoMessageType NicoMessageType { get; } = NicoMessageType.Kick;
        public NicoKickCommand(string raw) : base(raw)
        {

        }
    }
}
