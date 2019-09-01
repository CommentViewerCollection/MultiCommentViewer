using System.Threading.Tasks;

namespace SitePluginCommon.AutoReconnector
{
    public interface IConnector
    {
        Task<bool> IsLivingAsync();
        Task<DisconnectReason> ConnectAsync();
        void Disconnect();
    }
}
