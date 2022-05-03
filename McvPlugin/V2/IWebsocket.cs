namespace Mcv.PluginV2;

public interface IWebsocket
{
    event EventHandler Opened;
    event EventHandler<string> Received;

    void Disconnect();
    Task ReceiveAsync(string url);
    Task SendAsync(string s);
}
