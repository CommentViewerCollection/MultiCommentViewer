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
        bool IsAutoSetNickname { get; set; }
    }
}
