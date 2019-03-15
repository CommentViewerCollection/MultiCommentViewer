using System;
using System.Collections.Generic;

namespace YouTubeLiveSitePlugin.Test2
{
    /// <summary>
    /// リロードの日時や回数を記録、管理
    /// </summary>
    class ReloadManager
    {
        /// <summary>
        /// 
        /// </summary>
        public int CountLimit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int CountCheckTimeRangeMin { get; set; }
        List<DateTime> _reloadTimeList = new List<DateTime>();
        public void SetTime()
        {
            _reloadTimeList.Add(GetCurrentDateTime());
        }
        public virtual bool CanReload()
        {
            if(CountLimit<= 0)
            {
                return false;
            }
            var a = _reloadTimeList.Count < CountLimit;
            if (a) return true;
            var b = _reloadTimeList[_reloadTimeList.Count - CountLimit];
            var c = GetCurrentDateTime();
            var d = new TimeSpan(0, CountCheckTimeRangeMin, 0);
            var cantReload= c - b < d;
            return !cantReload;
        }
        public virtual DateTime GetCurrentDateTime()
        {
            return DateTime.Now;
        }
    }
}
