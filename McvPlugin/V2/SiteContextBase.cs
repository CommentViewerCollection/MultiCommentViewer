using System.Windows.Controls;

namespace Mcv.PluginV2;

public abstract class SiteContextBase : ISiteContext
{
    private readonly ILogger _logger;

    protected abstract SiteType SiteType { get; }
    public abstract string DisplayName { get; }
    public abstract IOptionsTabPage TabPanel { get; }

    public abstract ICommentProvider CreateCommentProvider();

    public abstract UserControl GetCommentPostPanel(ICommentProvider commentProvider);

    public abstract bool IsValidInput(string input);

    public abstract void LoadOptions(string path, IIo io);

    public abstract void SaveOptions(string path, IIo io);
    public abstract void LoadOptions(string rawOptions);
    public abstract string GetSiteOptions();
    public SiteContextBase(ILogger logger)
    {
        _logger = logger;
    }
}
