using System;
using System.ComponentModel;
using System.Windows.Media;

namespace LineLiveSitePlugin
{
    interface ISiteOptions<T>:INotifyPropertyChanged
    {
        T Clone();
        void Set(T t);
    }
    interface ILineLiveSiteOptions: ISiteOptions<ILineLiveSiteOptions>
    {
        bool IsAutoSetNickname { get; set; }
    }
}
