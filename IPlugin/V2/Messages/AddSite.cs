using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Mcv.PluginV2.Messages
{
    public class NotifySiteAdded : INotifyMessageV2
    {
        public NotifySiteAdded(SiteId siteId, string siteDisplayName)
        {
            SiteId = siteId;
            SiteDisplayName = siteDisplayName;
        }

        public SiteId SiteId { get; }
        public string SiteDisplayName { get; }
    }
}
