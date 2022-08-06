using System.ComponentModel;
using System.Windows.Media;

namespace TwitchSitePlugin
{
    interface ITwitchSiteOptions : INotifyPropertyChanged
    {
        bool NeedAutoSubNickname { get; }
        string NeedAutoSubNicknameStr { get; }
        Color NoticeBackColor { get; }
        Color NoticeForeColor { get; }
    }
}
