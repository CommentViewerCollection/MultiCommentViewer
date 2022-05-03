using System.Collections.Generic;

namespace Mcv.PluginV2.Messages
{
    public class NotifyPluginInfoList : INotifyMessageV2
    {
        public NotifyPluginInfoList(List<IPluginInfo> plugins)
        {
            Plugins = plugins;
        }

        public List<IPluginInfo> Plugins { get; }
        public string Raw
        {
            get
            {
                return "";
            }
        }
    }
}