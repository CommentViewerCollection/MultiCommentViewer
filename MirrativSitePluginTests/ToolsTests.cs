using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MirrativSitePluginTests
{
    [TestFixture]
    class ToolsTests
    {
        [Test]
        public void ExtractLiveIdTest()
        {
            Assert.AreEqual("sUbSZSYyTAOYJtLd0WamoQ", MirrativSitePlugin.Tools.ExtractLiveId("https://www.mirrativ.com/live/sUbSZSYyTAOYJtLd0WamoQ?test"));
            Assert.AreEqual("sUbSZSYyTAOYJtLd0WamoQ", MirrativSitePlugin.Tools.ExtractLiveId("https://www.mirrativ.com/broadcast/sUbSZSYyTAOYJtLd0WamoQ?test"));
        }
        [Test]
        public void KeyValue2DictTest()
        {
            var str = "{\"key1\":\"value1\",\"key2\":0,\"key3\":\"value3\"}";
            var dict = MirrativSitePlugin.Tools.KeyValue2Dict(str);
            Assert.AreEqual(3, dict.Count);
            Assert.IsTrue(dict.ContainsKey("key1"));
            Assert.AreEqual("value1", dict["key1"]);
            Assert.IsTrue(dict.ContainsKey("key2"));
            Assert.AreEqual("0", dict["key2"]);
            Assert.IsTrue(dict.ContainsKey("key3"));
            Assert.AreEqual("value3", dict["key3"]);
        }
    }
}
