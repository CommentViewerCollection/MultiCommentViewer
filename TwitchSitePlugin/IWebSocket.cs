using System;
using System.Threading.Tasks;
namespace TwitchSitePlugin
{
    public interface IMessageProvider
    {
        event EventHandler<Result> Received;
        event EventHandler Opened;
        Task SendAsync(string s);
        Task ReceiveAsync();
        void Disconnect();
    }
}
