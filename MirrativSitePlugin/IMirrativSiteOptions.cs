using System.ComponentModel;
using System.Windows.Media;

namespace MirrativSitePlugin
{
    interface IMirrativSiteOptions:INotifyPropertyChanged
    {
        bool NeedAutoSubNickname { get; }
        Color ItemForeColor { get; }
        Color ItemBackColor { get; }
        int PollingIntervalSec { get; }
    }
}
