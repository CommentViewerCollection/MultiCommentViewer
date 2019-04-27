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
        public void ExtractLiveIdTest()
        {
            Assert.AreEqual("1ypJdvRwXQVKW", Tools.ExtractLiveId("https://www.pscp.tv/w/1ypJdvRwXQVKW?channel=fave-musician"));
            Assert.IsNull(Tools.ExtractLiveId("a"));
        }
        [Test]
        public void ExtractHostnameFromEndpointTest()
        {
            Assert.AreEqual("prod-chatman-ancillary-us-east-1.pscp.tv", Tools.ExtractHostnameFromEndpoint("https://prod-chatman-ancillary-us-east-1.pscp.tv"));
        }
    }
}
