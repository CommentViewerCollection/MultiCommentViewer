using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NicoSitePlugin;
namespace NicoSitePluginTests
{
    [TestFixture]
    class ToolsTests
    {
        [Test]
        public void Is184UserIdTest()
        {
            Assert.IsTrue(Tools.Is184UserId("edJpZLUT3Fk-gRGaw34IXGnr8zc"));
            Assert.IsFalse(Tools.Is184UserId("123456"));
        }
        [Test]
        public void ExtractChannelScreenNameTest()
        {
            Assert.AreEqual("access-ann", Tools.ExtractChannelScreenName("http://ch.nicovideo.jp/access-ann"));
            Assert.AreEqual(null, Tools.ExtractChannelScreenName("abc"));
        }
        [Test]
        public void Nico_GetShortRoomNameTest()
        {
            Assert.AreEqual("ｱ", Tools.GetShortRoomName("ch123"));
            Assert.AreEqual("ｱ", Tools.GetShortRoomName("co123"));
            Assert.AreEqual("A", Tools.GetShortRoomName("立ち見A列"));
            Assert.AreEqual("[", Tools.GetShortRoomName("立ち見[列"));
        }
        [Test]
        public void Nico_DistinctTest()
        {
            CollectionAssert.AreEquivalent(new List<int> { 3 }, Tools.Distinct<int>(new List<int> { 1, 2 }, new List<int> { 1, 2, 3 }));

            var expected = new List<RoomInfo>
            {
                new RoomInfo(new MsTest("c", 2, "2"),"c"),
            };
            var main = new List<RoomInfo>
            {
                new RoomInfo(new MsTest("a", 0, "0"),"a"),
                new RoomInfo(new MsTest("b", 1, "1"),"b"),
            };
            var newList = new List<RoomInfo>
            {
                new RoomInfo(new MsTest("a", 0, "0"),"a"),
                new RoomInfo(new MsTest("b", 1, "1"),"b"),
                new RoomInfo(new MsTest("c", 2, "2"),"c"),
            };
            var actual = Tools.Distinct(main, newList);
            CollectionAssert.AreEquivalent(expected, actual);
        }
    }
}
