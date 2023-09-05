using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Moq;
using OpenrecSitePlugin;
using Newtonsoft.Json;
namespace OpenrecSitePluginTests
{
    [TestFixture]
    class ToolsTests
    {
        [Test]
        public async Task GetLiveIdTest()
        {
            //https://www.openrec.tv/live/FkJTQSjkQps
            //https://www.openrec.tv/user/Matomo_SV
            //https://www.openrec.tv/live/Matomo_SV

            var data = DataLoader.GetSampleData(@"External\Movies\TextFile1.txt");
            var serverMock = new Mock<IDataSource>();
            serverMock.Setup(s => s.GetAsync("https://public.openrec.tv/external/api/v5/movies?channel_id=FkJTQSjkQps")).ReturnsAsync("[" + "]");
            serverMock.Setup(s => s.GetAsync("https://public.openrec.tv/external/api/v5/movies?channel_id=Matomo_SV")).ReturnsAsync("[" + data + "]");
            Assert.AreEqual("FkJTQSjkQps", await Tools.GetLiveId(serverMock.Object, "https://www.openrec.tv/live/FkJTQSjkQps"));
            Assert.AreEqual("FkJTQSjkQps", await Tools.GetLiveId(serverMock.Object, "https://www.openrec.tv/user/Matomo_SV"));
            Assert.AreEqual("FkJTQSjkQps", await Tools.GetLiveId(serverMock.Object, "https://www.openrec.tv/live/Matomo_SV"));

        }
        [Test]
        public void IsValidUrlTest()
        {
            Assert.IsTrue(Tools.IsValidUrl("https://www.openrec.tv/live/mizLXCyaDLk"));
            Assert.IsTrue(Tools.IsValidUrl("https://www.openrec.tv/movie/mizLXCyaDLk"));
            Assert.IsFalse(Tools.IsValidUrl("https://www.openrec.tv/a/mizLXCyaDLk"));

        }
        [Test]
        public void ExtractLiveIdTest()
        {
            Assert.AreEqual("mizLXCyaDLk", Tools.ExtractLiveId("https://www.openrec.tv/live/mizLXCyaDLk"));
            Assert.AreEqual("mizLXCyaDLk", Tools.ExtractLiveId("https://www.openrec.tv/movie/mizLXCyaDLk"));
            Assert.AreEqual("", Tools.ExtractLiveId("https://www.openrec.tv/a/mizLXCyaDLk"));
        }
        [Test]
        public void WebsocketMessageType0ParseTest()
        {
            var data = DataLoader.GetSampleData("Websocket\\MessageType0.txt");
            var obj = JsonConvert.DeserializeObject<OpenrecSitePlugin.Low.Item>(data);
            var comment = Tools.Parse(obj);
            Assert.AreEqual("運だけ〜", comment.Message);
            Assert.AreEqual("182743145", comment.Id);
            Assert.IsTrue(comment.IsPremium);
        }
        [Test]
        public void ChatsParseTest()
        {
            var data = DataLoader.GetSampleData("Chats_stamp.txt");
            var obj = JsonConvert.DeserializeObject<OpenrecSitePlugin.Low.Chats.RootObject[]>(data);
            var comment = Tools.Parse(obj[10]);
            Assert.AreEqual("", comment.Message);
            Assert.AreEqual("182716576", comment.Id);
            Assert.IsTrue(comment.IsPremium);
            Assert.AreEqual("https://dqd0jw5gvbchn.cloudfront.net/stamp/15/128/16a1341ae3788fa55c84cb05be3663825f5df7e2.png", comment.StampUrl);
        }
    }
}
