using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Mcv.PluginV2;

namespace McvCore;

static class PluginLoader
{
    private static bool IsValidPluginFileName(string filePath)
    {
        return filePath.EndsWith("Plugin.dll") && filePath is not "Plugin.dll" && filePath is not "McvPlugin.dll";
    }
    private static IPlugin? LoadPluginsV2(string file)
    {
        var catalog = new AssemblyCatalog(file);
        var con = new CompositionContainer(catalog);
        var t = new ImportDefinition(ed => true, "", ImportCardinality.ZeroOrMore, false, true);
        var dd = con.GetExports(t);
        foreach (var a in dd.ToList())//リストにしているけど、0個か1個しかない。
        {
            if (a.Value is IPlugin v2)
            {
                return v2;
            }
        }
        return null;
    }
    public static List<IPlugin> LoadPlugins(string pluginsDir)
    {
        var pluginDirs = Directory.GetDirectories(pluginsDir);
        var plugins = new List<IPlugin>();
        foreach (var pluginDir in pluginDirs)
        {
            var files = Directory.GetFiles(pluginDir).Where(s => IsValidPluginFileName(s));
            foreach (var filePath in files)
            {
                try
                {
                    var plugin = LoadPluginsV2(filePath);
                    if (plugin is null) continue;
                    plugins.Add(plugin);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }
        return plugins;
    }
}
