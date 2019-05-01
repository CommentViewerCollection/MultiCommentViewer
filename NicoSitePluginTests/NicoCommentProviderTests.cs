using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Moq;
using NicoSitePlugin;
using NUnit.Framework;
using SitePlugin;
using SitePluginCommon;

namespace NicoSitePluginTests
{
    [TestFixture]
    class NicoCommentProviderTests
    {
        [Test]
        public void GetAdCommentWithMessageTest()
        {
            Assert.AreEqual("提供：無料チケマン「無料チケット捨てに来ました！」（200pt）", Tools.GetAdComment("/nicoad {\"latestNicoad\":{\"advertiser\":\"無料チケマン\",\"point\":200,\"message\":\"無料チケット捨てに来ました！\"},\"contributionRanking\":{\"totalPoint\":10800,\"ranking\":[{\"advertiser\":\"クリームパンナちゃん\",\"rank\":1,\"message\":\"かがーさんの楽しい旅♪\"},{\"advertiser\":\"ゲスト\",\"rank\":2},{\"advertiser\":\"ケイジ\",\"rank\":3},{\"advertiser\":\"キエル\",\"rank\":4},{\"advertiser\":\"無料チケマン\",\"rank\":5}]}}"));
        }
        [Test]
        public void GetAdCommentWithoutMessageTest()
        {
            Assert.AreEqual("提供：無料チケマン（200pt）", Tools.GetAdComment("/nicoad {\"latestNicoad\":{\"advertiser\":\"無料チケマン\",\"point\":200},\"contributionRanking\":{\"totalPoint\":10800,\"ranking\":[{\"advertiser\":\"クリームパンナちゃん\",\"rank\":1,\"message\":\"かがーさんの楽しい旅♪\"},{\"advertiser\":\"ゲスト\",\"rank\":2},{\"advertiser\":\"ケイジ\",\"rank\":3},{\"advertiser\":\"キエル\",\"rank\":4},{\"advertiser\":\"無料チケマン\",\"rank\":5}]}}"));
        }
    }
    [TestFixture]
    class Tests
    {
        [Test]
        public async Task Test()
        {
            var optionsMock = new Mock<ICommentOptions>();
            var siteOptionsMock = new Mock<INicoSiteOptions>();
            siteOptionsMock.Setup(s => s.IsAutoSetNickname).Returns(true);
            var userStoreMock = new Mock<IUserStoreManager>();
            var userId = "G-lRat9seQmpK-gcgcQXSFxr14c";
            var user = new UserTest(userId);
            userStoreMock.Setup(s => s.GetUser(SiteType.NicoLive, userId)).Returns(user);

            var serverMock = new Mock<IDataSource>();
            var loggerMock = new Mock<ILogger>();
            var cpMock = new Mock<ICommentProvider>();

            var provider = new CommunityCommentProvider(optionsMock.Object, siteOptionsMock.Object, userStoreMock.Object, serverMock.Object, loggerMock.Object, cpMock.Object);
            var chat = new Chat("<chat thread=\"1645724171\" no=\"4\" vpos=\"180000\" date=\"1550890471\" date_usec=\"549074\" mail=\"184\" user_id=\"G-lRat9seQmpK-gcgcQXSFxr14c\" premium=\"1\" anonymity=\"1\" locale=\"ja-jp\">@test</chat>");
            var roomName = "ch123";
            var messageContext = await provider.CreateMessageContextAsync(chat, roomName, false);
            Assert.AreEqual("test", user.Nickname);
        }
    }
}
