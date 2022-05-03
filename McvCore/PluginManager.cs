using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mcv.PluginV2.Messages;
using Mcv.PluginV2;
using Org.BouncyCastle.Asn1.X509;

namespace McvCore;
class PluginManager
{
    public void AddPlugin(IPlugin plugin, IPluginHost host)
    {
        _pluginsV2.Add(plugin);
        plugin.Host = host;
        plugin.SetMessage(new SetLoading());
        //Plugin側がHelloメッセージを送ってくるまで登録はしない。
    }
    public void AddPlugins(List<IPlugin> plugins, IPluginHost host)
    {
        foreach (var plugin in plugins)
        {
            AddPlugin(plugin, host);
        }
    }
    internal List<IPluginInfo> GetPluginList()
    {
        return _pluginsV2.Cast<IPluginInfo>().ToList();
    }

    private readonly List<IPlugin> _pluginsV2 = new();
    internal void SetMessage(INotifyMessageV2 message)
    {
        foreach (var plugin in _pluginsV2)
        {
            plugin.SetMessage(message);
        }
    }
    internal void SetMessage(PluginId targetId, INotifyMessageV2 message)
    {
        var target = GetPluginById(targetId);
        if (target == null)
        {
            return;
        }
        target.SetMessage(message);
    }
    internal void SetMessage(PluginId targetId, ISetMessageToPluginV2 message)
    {
        var target = GetPluginById(targetId);
        if (target == null)
        {
            return;
        }
        target.SetMessage(message);
    }
    internal void SetMessage(ISetMessageToPluginV2 message)
    {
        foreach (var plugin in _pluginsV2)
        {
            plugin.SetMessage(message);
        }
    }





    //public void OnClosing()
    //{
    //    if (_pluginsV2 == null)
    //        return;
    //    foreach (var plugin in _pluginsV2)
    //    {
    //        try
    //        {
    //            plugin.OnClosing();
    //        }
    //        catch (Exception ex)
    //        {
    //            Debug.WriteLine(ex.Message);
    //        }
    //    }
    //}

    //public void ForeachPlugin(Action<PluginV0.IPlugin> p)
    //{
    //    foreach (var plugin in _pluginsV2)
    //    {
    //        try
    //        {
    //            p(plugin);
    //        }
    //        catch (Exception ex)
    //        {
    //            Debug.WriteLine(ex.Message);
    //        }
    //    }
    //}

    //public void SetMessage(ISiteMessage message, IMessageMetadata messageMetadata)
    //{
    //    foreach (var plugin in _pluginsV2)
    //    {
    //        plugin.OnMessageReceived(message, messageMetadata);
    //    }
    //}

    //public void OnTopmostChanged(bool isTopmost)
    //{
    //    foreach (var plugin in _pluginsV2)
    //    {
    //        plugin.OnTopmostChanged(isTopmost);
    //    }
    //}

    //private readonly IMcvCoreOptions _options;
    //public PluginManager()
    //{
    //    //_options = options;
    //}



    public PluginManager()
    {
    }
    internal PluginId? GetDefaultSite()
    {
        return _pluginRoleDict.Where(p => PluginTypeChecker.IsSitePlugin(p.Value)).Select(p => p.Key).FirstOrDefault();
    }
    internal IReplyMessageToPluginV2 RequestMessage(PluginId pluginId, IGetMessageToPluginV2 message)
    {
        var plugin = GetPluginById(pluginId);
        if (plugin is null)
        {
            return null;//TODO:
        }
        return plugin.RequestMessage(message);
    }
    private IPlugin? GetPluginById(PluginId pluginId)
    {
        return _pluginsV2.Find(p => p.Id == pluginId)!;
    }
    private readonly Dictionary<PluginId, IList<string>> _pluginRoleDict = new();
    internal void SetPluginRole(PluginId pluginId, List<string> pluginRole)
    {
        _pluginRoleDict.Add(pluginId, pluginRole);
    }

    internal void RemovePlugin(PluginId pluginId)
    {
        try
        {
            _pluginRoleDict.Remove(pluginId);
            var plugin = GetPluginById(pluginId);
            _pluginsV2.Remove(plugin);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    internal IPluginInfo? GetPluginInfo(PluginId pluginId)
    {
        var plugin = GetPluginById(pluginId);
        return plugin;
    }
}
