using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SitePlugin;
using NicoSitePlugin;
using ryu_s.BrowserCookie;

namespace MultiCommentViewer.Test.NicoLive
{
    class NicoCommentProviderOld : ICommentProvider
    {
        public bool CanConnect => throw new NotImplementedException();

        public bool CanDisconnect => throw new NotImplementedException();

        public event EventHandler<List<ICommentViewModel>> CommentsReceived;
        public event EventHandler<IMetadata> MetadataUpdated;
        public event EventHandler CanConnectChanged;
        public event EventHandler CanDisconnectChanged;

        public Task ConnectAsync(string input, IBrowserProfile browserProfile)
        {
            //TODO:新配信か旧配信かを判別する必要がある。
            //とりあえず旧配信から実装しよう。
            throw new NotImplementedException();
        }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public List<ICommentViewModel> GetUserComments(IUser user)
        {
            throw new NotImplementedException();
        }

        public Task PostCommentAsync(string text)
        {
            throw new NotImplementedException();
        }
    }
}
