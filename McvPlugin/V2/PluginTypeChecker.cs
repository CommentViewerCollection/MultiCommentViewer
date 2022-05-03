namespace Mcv.PluginV2;

public static class PluginTypeChecker
{
    public static bool IsSitePlugin(IList<string> pluginRoles)
    {
        return pluginRoles.Any(s => s.StartsWith("site:"));
    }
    public static bool IsBrowserPlugin(IList<string> pluginRoles)
    {
        return pluginRoles.Any(s => s.StartsWith("cookie:"));
    }
}