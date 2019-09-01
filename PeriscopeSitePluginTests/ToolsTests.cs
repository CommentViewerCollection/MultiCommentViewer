using NUnit.Framework;
using PeriscopeSitePlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeriscopeSitePluginTests
{
    [TestFixture]
    class ToolsTests
    {
        [Test]
        public void ExtractChannelNameAndLiveIdTest()
        {
            Assert.AreEqual(("fave-musician", "1ypJdvRwXQVKW"), Tools.ExtractChannelNameAndLiveId("https://www.periscope.tv/fave-musician/1ypJdvRwXQVKW"));
            Assert.AreEqual(("fave-musician", "1ypJdvRwXQVKW"), Tools.ExtractChannelNameAndLiveId("https://www.periscope.tv/w/1ypJdvRwXQVKW?channel=fave-musician"));
            Assert.AreEqual(("fave-musician", "1ypJdvRwXQVKW"), Tools.ExtractChannelNameAndLiveId("https://www.pscp.tv/w/1ypJdvRwXQVKW?channel=fave-musician"));
            Assert.AreEqual(((string)null, "1ypJdvRwXQVKW"), Tools.ExtractChannelNameAndLiveId("https://www.periscope.tv/w/1ypJdvRwXQVKW"));
            Assert.AreEqual(((string)null, "1ypJdvRwXQVKW"), Tools.ExtractChannelNameAndLiveId("https://www.pscp.tv/w/1ypJdvRwXQVKW"));
            Assert.AreEqual(((string)null, (string)null), Tools.ExtractChannelNameAndLiveId("a"));
        }
        [Test]
        public void ExtractHostnameFromEndpointTest()
        {
            Assert.AreEqual("prod-chatman-ancillary-us-east-1.pscp.tv", Tools.ExtractHostnameFromEndpoint("https://prod-chatman-ancillary-us-east-1.pscp.tv"));
        }
    }
}
