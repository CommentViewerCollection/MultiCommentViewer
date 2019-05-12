using System;
using System.ComponentModel;
using System.Windows.Media;

namespace PeriscopeSitePlugin
{
    interface ISiteOptions<T>:INotifyPropertyChanged
    {
        T Clone();
        void Set(T t);
    }
    interface IPeriscopeSiteOptions: ISiteOptions<IPeriscopeSiteOptions>
    {
        Color ItemCommentBackColor { get; set; }
        Color ItemCommentForeColor { get; set; }
        bool IsAutoSetNickname { get; set; }
        bool IsShowJoinMessage { get; set; }
        bool IsShowLeaveMessage { get; set; }
    }
}
