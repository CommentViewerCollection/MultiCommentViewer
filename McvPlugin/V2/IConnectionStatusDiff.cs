namespace Mcv.PluginV2
{

    public interface IConnectionStatusDiff
    {
        ConnectionId Id { get; }
        string? Name { get; set; }
        //string? Input { get; set; }
        PluginId? SelectedSite { get; set; }
        BrowserProfileId? SelectedBrowser { get; set; }
        bool? IsConnected { get; set; }
        bool? CanConnect { get; set; }
        bool? CanDisconnect { get; set; }
        //bool IsChanged()
        //{
        //    var props = GetType().GetProperties();
        //    foreach (var prop in props)
        //    {
        //        var val = prop.GetValue(this);
        //        if (val != null)
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}
    }
    public static class ConnectionStatusDiffExtension
    {
        public static bool IsChanged(this IConnectionStatusDiff diff)
        {
            var props = diff.GetType().GetProperties();
            foreach (var prop in props)
            {
                var val = prop.GetValue(diff);
                if (val != null)
                {
                    return true;
                }
            }
            return false;
        }
    }

}
