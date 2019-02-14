using System;
using System.Threading.Tasks;

namespace SitePluginCommon
{
    public interface IActiveCounter<T>
    {
        /// <summary>
        /// アクティブが更新された
        /// </summary>
        event EventHandler<int> Updated;
        /// <summary>
        /// アクティブの更新間隔（秒）
        /// </summary>
        int CountIntervalSec { get; set; }
        /// <summary>
        /// 直近何分間のアクティブを集計するか
        /// </summary>
        int MeasureSpanMin { get; set; }
        /// <summary>
        /// アクティブの計測を開始
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task Start();
        /// <summary>
        /// 対象の動作があった
        /// </summary>
        void Add(T id);

        void Stop();
    }
}
