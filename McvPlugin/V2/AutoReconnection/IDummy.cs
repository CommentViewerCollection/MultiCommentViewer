using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mcv.PluginV2.AutoReconnection
{
    /// <summary>
    /// 名称未設定
    /// </summary>
    public interface IDummy
    {
        Task<bool> CanConnectAsync();
        Task<IEnumerable<IProvider>> GenerateGroupAsync();
    }
}
