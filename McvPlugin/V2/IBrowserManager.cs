namespace Mcv.PluginV2;

public interface IBrowserManager
{
    BrowserType Type { get; }
    List<IBrowserProfile> GetProfiles();
}
public interface IIEManager : IBrowserManager { }
public interface IFirefoxManager : IBrowserManager { }
public interface IChromeManager : IBrowserManager { }
public interface IEdgeManager : IBrowserManager { }
public interface IOperaManager : IBrowserManager { }
