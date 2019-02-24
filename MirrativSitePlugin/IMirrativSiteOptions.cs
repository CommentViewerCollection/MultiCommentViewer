using System.ComponentModel;

namespace MirrativSitePlugin
{
    interface IMirrativSiteOptions:INotifyPropertyChanged
    {
        bool NeedAutoSubNickname { get; }
        int PollingIntervalSec { get; }
    }
}
