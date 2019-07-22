using System;
using System.ComponentModel;
using System.Windows.Media;

namespace ShowRoomSitePlugin
{
    interface ISiteOptions<T>:INotifyPropertyChanged
    {
        T Clone();
        void Set(T t);
    }
    interface IShowRoomSiteOptions: ISiteOptions<IShowRoomSiteOptions>
    {
        Color ItemCommentBackColor { get; set; }
        Color ItemCommentForeColor { get; set; }
        bool IsAutoSetNickname { get; set; }
        bool IsShowJoinMessage { get; set; }
        bool IsShowLeaveMessage { get; set; }
        bool IsIgnore50Counts { get; set; }
    }
}
