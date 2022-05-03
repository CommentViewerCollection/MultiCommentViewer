using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mcv.PluginV2.Messages
{
    public class NotifyBrowserAdded : INotifyMessageV2
    {
        public NotifyBrowserAdded(BrowserProfileId browserProfileId, string browserDisplayName, string profileDisplayName)
        {
            BrowserProfileId = browserProfileId;
            BrowserDisplayName = browserDisplayName;
            ProfileDisplayName = profileDisplayName;
        }

        public BrowserProfileId BrowserProfileId { get; }
        public string BrowserDisplayName { get; }
        public string ProfileDisplayName { get; }
    }
}
