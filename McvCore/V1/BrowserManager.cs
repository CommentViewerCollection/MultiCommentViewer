using Mcv.PluginV2;
using ryu_s.BrowserCookie;
using System;
using System.Collections.Generic;
using System.Linq;

namespace McvCore
{
    class BrowserAddedEventArgs : EventArgs
    {
        public BrowserAddedEventArgs(BrowserProfileId browserProfileId,string browserDisplayName,string profileDisplayName)
        {
            BrowserProfileId = browserProfileId;
            BrowserDisplayName = browserDisplayName;
            ProfileDisplayName = profileDisplayName;
        }

        public BrowserProfileId BrowserProfileId { get; }
        public string BrowserDisplayName { get; }
        public string ProfileDisplayName { get; }
    }
    class BrowserManager
    {
        public event EventHandler<BrowserAddedEventArgs>? BrowserAdded;
        //BrowserProfileは動的に生成される。IDは最初は生成して、保存しておきたい。IDとpathをセットにして保存すれば良さそう。
        public void LoadBrowserProfiles()
        {
            var list = new List<IBrowserProfile>();
            var managers = new List<IBrowserManager>
            {
                new ChromeManager(),
                new ChromeBetaManager(),
                new FirefoxManager(),
                new EdgeManager(),
                new OperaManager(),
                new OperaGxManager(),
            };
            foreach (var manager in managers)
            {
                try
                {
                    list.AddRange(manager.GetProfiles());
                }
                catch (Exception ex)
                {
                    //_logger.LogException(ex);
                }
            }
            foreach(var profile in list)
            {
                var id = new BrowserProfileId(Guid.NewGuid());
                _dict.Add(id, profile);
                BrowserAdded?.Invoke(this, new BrowserAddedEventArgs(id, profile.Type.ToString(), profile.ProfileName));
            }
        }
        Dictionary<BrowserProfileId, IBrowserProfile> _dict = new Dictionary<BrowserProfileId, IBrowserProfile>();
        internal BrowserProfileId GetDefaultBrowser()
        {
            var first = _dict.Select(kv => kv.Key).ToList()[0];
            return first;
        }
        public IBrowserProfile GetBrowserProfile(BrowserProfileId browserProfileId)
        {
            return _dict[browserProfileId];
        }
    }

}
