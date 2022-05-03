using System;
using System.Net;
using System.Threading.Tasks;

namespace Mcv.PluginV2;

public abstract class CommentProviderBase : ICommentProvider
{
    private bool _canConnect;
    public bool CanConnect
    {
        get { return _canConnect; }
        set
        {
            if (_canConnect == value)
                return;
            _canConnect = value;
            CanConnectChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private bool _canDisconnect;
    private readonly ILogger _logger;

    public bool CanDisconnect
    {
        get { return _canDisconnect; }
        set
        {
            if (_canDisconnect == value)
                return;
            _canDisconnect = value;
            CanDisconnectChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    protected virtual CookieContainer CreateCookieContainer(List<Cookie> cookies)
    {
        var cc = new CookieContainer();
        try
        {
            foreach (var cookie in cookies)
            {
                cc.Add(cookie);
            }
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
        }
        return cc;
    }
    protected void SendSystemInfo(string message, InfoType type)
    {
        var context = InfoMessageContext.Create(new InfoMessage
        {
            Text = message,
            SiteType = SiteType.Periscope,
            Type = type,
        });
        MessageReceived?.Invoke(this, context);
    }
    public event EventHandler<IMetadata>? MetadataUpdated;
    public event EventHandler? CanConnectChanged;
    public event EventHandler? CanDisconnectChanged;
    public event EventHandler<ConnectedEventArgs>? Connected;
    public event EventHandler<IMessageContext>? MessageReceived;
    protected void RaiseMessageReceived(IMessageContext context)
    {
        MessageReceived?.Invoke(this, context);
    }
    protected void RaiseMetadataUpdated(IMetadata metadata)
    {
        MetadataUpdated?.Invoke(this, metadata);
    }
    public abstract Task ConnectAsync(string input, List<Cookie> browserProfile);

    public abstract void Disconnect();

    public abstract Task PostCommentAsync(string text);

    public abstract Task<ICurrentUserInfo> GetCurrentUserInfo(List<Cookie> browserProfile);
    protected virtual void BeforeConnect()
    {
        CanConnect = false;
        CanDisconnect = true;
    }
    protected virtual void AfterDisconnected()
    {
        CanConnect = true;
        CanDisconnect = false;
    }

    public abstract void SetMessage(string raw);

    public CommentProviderBase(ILogger logger)
    {
        _logger = logger;

        CanConnect = true;
        CanDisconnect = false;
    }
}
