using System.Threading.Tasks;

namespace SitePluginCommon.AutoReconnection
{
    public interface IProvider
    {
        /// <summary>
        /// Masterが終了した場合、自身の存在価値が無くなる
        /// </summary>
        IProvider Master { get; }
        bool IsFinished { get; }
        void Start();
        void Stop();
        Task Work { get; }
        ProviderFinishReason FinishReason { get; }
    }
}
