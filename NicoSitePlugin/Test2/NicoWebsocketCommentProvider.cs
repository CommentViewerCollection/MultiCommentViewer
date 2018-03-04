using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NicoSitePlugin.Old;
using ryu_s.BrowserCookie;
using SitePlugin;
using NicoSitePlugin.Websocket;
namespace NicoSitePlugin.Test2
{
    class NicoWebsocketCommentProvider : INicoCommentProvider
    {
        public bool CanConnect
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool CanDisconnect
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public event EventHandler<List<ICommentViewModel>> InitialCommentsReceived;
        public event EventHandler<ICommentViewModel> CommentReceived;
        public event EventHandler<IMetadata> MetadataUpdated;
        public event EventHandler CanConnectChanged;
        public event EventHandler CanDisconnectChanged;

        public Task ConnectAsync(string input, IBrowserProfile browserProfile)
        {
            throw new NotImplementedException();
        }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ICommentViewModel> GetUserComments(IUser user)
        {
            throw new NotImplementedException();
        }

        public Task PostCommentAsync(string text, string mail)
        {
            throw new NotImplementedException();
        }
    }
}
