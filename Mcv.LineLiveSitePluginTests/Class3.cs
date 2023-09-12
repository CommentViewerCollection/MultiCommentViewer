using LineLiveSitePlugin;
using Mcv.PluginV2;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace LineLiveSitePluginTests
{
    //LineLiveCommentProviderのBlackListProviderに対する反応をテストしたい

    class TestBlackListProvider : IBlackListProvider
    {
        public event EventHandler<long[]> Received;

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public Task ReceiveAsync(List<Cookie> cookies)
        {
            throw new NotImplementedException();
        }
    }
    class Class3 : LineLiveCommentProvider
    {
        public Class3(IDataServer server, ILogger logger, LineLiveSiteOptions siteOptions)
            : base(server, logger, siteOptions)
        {

        }
        public void SetNgUserPub(long[] old, long[] @new)
        {
            base.SetNgUser(old, @new);
        }
    }
}
