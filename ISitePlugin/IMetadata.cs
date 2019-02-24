using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SitePlugin
{
    public interface IMetadata
    {  
        //タイトル
        string Title { get; }
        //放送経過時間
        string Elapsed { get; }
        //視聴者数
        string CurrentViewers { get; }
        //アクティブ
        string Active { get; }
        /// <summary>
        /// 総来場者数
        /// </summary>
        string TotalViewers { get; }

        bool? IsLive { get; }
    }
}
