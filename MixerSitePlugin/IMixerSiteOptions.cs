using System.ComponentModel;
using System.Windows.Media;

namespace MixerSitePlugin
{
    interface IMixerSiteOptions:INotifyPropertyChanged
    {
        bool NeedAutoSubNickname { get; }
        bool IsShowJoinMessage { get; }
        bool IsShowLeaveMessage { get; }
        Color ItemForeColor { get; }
        Color ItemBackColor { get; }
        int PollingIntervalSec { get; }
    }
}
