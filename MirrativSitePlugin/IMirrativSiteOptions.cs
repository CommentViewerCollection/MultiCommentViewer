namespace MirrativSitePlugin
{
    interface IMirrativSiteOptions
    {
        bool NeedAutoSubNickname { get; }
        int PollingIntervalSec { get; }
    }
}
