using Codeplex.Data;
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
            Assert.AreEqual(("fave-musician", "1ypJdvRwXQVKW"), Tools.ExtractChannelNameAndLiveId("https://www.pscp.tv/fave-musician/1ypJdvRwXQVKW"));
            Assert.AreEqual(("fave-musician", (string)null), Tools.ExtractChannelNameAndLiveId("https://www.pscp.tv/fave-musician"));
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
        [Test]
        public void UrlTest()
        {
            var livePageUrl = Tools.GetUrl("https://www.pscp.tv/JLSwitzerland/1vAxRqWmZLXJl") as LivePageUrl;
            Assert.IsNotNull(livePageUrl);
            Assert.AreEqual("https://www.pscp.tv/JLSwitzerland/1vAxRqWmZLXJl", livePageUrl.Url);
            Assert.AreEqual("1vAxRqWmZLXJl", livePageUrl.LiveId);
            Assert.AreEqual("JLSwitzerland", livePageUrl.ChannelId);

            var channelUrl = Tools.GetUrl("https://www.pscp.tv/JLSwitzerland") as ChannelUrl;
            Assert.IsNotNull(channelUrl);
            Assert.AreEqual("https://www.pscp.tv/JLSwitzerland", channelUrl.Url);
            Assert.AreEqual("JLSwitzerland", channelUrl.ChannelId);

            var unknownUrl = Tools.GetUrl("abc") as UnknownUrl;
            Assert.IsNotNull(unknownUrl);
            Assert.AreEqual("abc", unknownUrl.Url);
        }
        [Test]
        public void ExtractChannelPageJsonTest()
        {
            var data = TestHelper.GetSampleData("ChannelPageHtml_Running.txt");
            var json = Tools.ExtractChannelPageJson(data);
            var d = DynamicJson.Parse(json);
            Assert.IsTrue(d.IsDefined("BroadcastCache"));
        }
    }
}
