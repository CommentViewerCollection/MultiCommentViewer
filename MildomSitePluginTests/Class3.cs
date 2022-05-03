using NUnit.Framework;

namespace MildomSitePluginTests
{
    [TestFixture]
    class Class3
    {
        [Test]
        public void GiftFindDeserializeTest()
        {
            var data = TestHelper.GetSampleData("gift_find.txt");
            var obj = MildomSitePlugin.Tools.Deserialize<MildomSitePlugin.Low.gift_find.RootObject>(data);
            Assert.AreEqual(1582386769094, obj.TsMs);
            Assert.AreEqual(1036, obj.Body.Models[0].GiftId);
            Assert.AreEqual("ライク", obj.Body.Models[0].Name);
            Assert.AreEqual("https://res.mildom.com/download/file/jp/mildom/nngift/821b5264777cecafd07fd6689ae5c973.png", obj.Body.Models[0].Pic);
            Assert.AreEqual(9, obj.Body.Models[0].Price);
        }
    }
}
