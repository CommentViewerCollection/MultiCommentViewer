using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhowatchSitePlugin;

namespace WhowatchSitePluginTests
{
    [TestFixture]
    class ToolsTests
    {
        [Test]
        public void IsValidUrlTest()
        {
            Assert.IsTrue(Tools.IsValidUrl("https://whowatch.tv/profile/w:kagawapro&test"));
            Assert.IsTrue(Tools.IsValidUrl("https://whowatch.tv/profile/t:kagawapro&test"));
            Assert.IsTrue(Tools.IsValidUrl("https://whowatch.tv/archives/6994269"));
            Assert.IsFalse(Tools.IsValidUrl("https://whowatch.tv/archives/"));
        }
        [Test]
        public void ExtractLiveIdFromInputTest()
        {
            Assert.AreEqual(7007970, Tools.ExtractLiveIdFromInput("https://whowatch.tv/viewer/7007970"));
        }
        [Test]
        public void ExtractUserPathFromInputTest()
        {
            Assert.AreEqual("w:kagawapro", Tools.ExtractUserPathFromInput("https://whowatch.tv/profile/w:kagawapro"));
            Assert.AreEqual("w:kagawapro", Tools.ExtractUserPathFromInput("w:kagawapro"));
            Assert.AreEqual("t:kagawapro", Tools.ExtractUserPathFromInput("t:kagawapro"));
            Assert.AreEqual("t:shinya_yuunari", Tools.ExtractUserPathFromInput("t:shinya_yuunari"));
            Assert.AreEqual("t:test123", Tools.ExtractUserPathFromInput("t:test123"));
            Assert.IsNull(Tools.ExtractUserPathFromInput(""));
        }

    }
}
