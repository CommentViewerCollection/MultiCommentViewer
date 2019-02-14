using System;
using System.Windows.Media;

namespace LineLiveSitePlugin
{
    interface ISiteOptions<T>
    {
        T Clone();
        void Set(T t);
    }
    interface ILineLiveSiteOptions: ISiteOptions<ILineLiveSiteOptions>
    {
        bool IsAutoSetNickname { get; set; }
    }
}
