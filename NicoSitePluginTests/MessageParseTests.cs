using Common;
using Moq;
using NicoSitePlugin;
using NUnit.Framework;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NicoSitePluginTests
{
    [TestFixture]
    class ChatParseTests
    {
        [Test]
        public async Task ParseKickCommandTest()
        {
            var chat = Chat.Parse("<chat thread=\"1649512882\" no=\"20464\" vpos=\"2138900\" date=\"1556823389\" date_usec=\"41570\" mail=\"184\" user_id=\"-Mp190qqmrjjfboRc5TSU-nhd8c\" premium=\"3\" anonymity=\"1\">/hb ifseetno 438</chat>");

            var userMock = new Mock<IUser>();
            var user = userMock.Object;

            var serverMock = new Mock<IDataSource>();
            var server = serverMock.Object;

            var loggerMock = new Mock<ILogger>();
            var logger = loggerMock.Object;

            var siteOptionsMock = new Mock<INicoSiteOptions>();
            var siteOptions = siteOptionsMock.Object;

            var message = new NicoKickCommand(chat.Raw);
            Assert.IsNotNull(message);

        }
        [Test]
        public async Task ParseInfoTest()
        {
            var chat = Chat.Parse("<chat thread=\"1650090280\" no=\"224\" vpos=\"571700\" date=\"1556890803\" date_usec=\"615687\" mail=\"184\" user_id=\"6dOBLLwJiMlr7es3qFL-EWkDQ6s\" premium=\"3\" anonymity=\"1\">/info 2 1人がコミュニティをフォローしました。</chat>");

            var userMock = new Mock<IUser>();
            var user = userMock.Object;

            var serverMock = new Mock<IDataSource>();
            var server = serverMock.Object;

            var loggerMock = new Mock<ILogger>();
            var logger = loggerMock.Object;

            var siteOptionsMock = new Mock<INicoSiteOptions>();
            var siteOptions = siteOptionsMock.Object;
            var message = Tools.CreateNicoInfo(chat, "roomname", siteOptions);
            Assert.IsNotNull(message);
            Assert.AreEqual("1人がコミュニティをフォローしました。", message.Text);
            Assert.AreEqual(2, message.No);
            Assert.AreEqual(NicoMessageType.Info, message.NicoMessageType);
        }
        [Test]
        public void ParseAdTest()
        {
            var chat = Chat.Parse("<chat thread=\"1650081308\" no=\"10965\" vpos=\"1607200\" date=\"1556892999\" date_usec=\"425525\" mail=\"184\" user_id=\"QkxkazwbTiKecwIxeASAfgkv_jQ\" premium=\"3\" anonymity=\"1\">/nicoad {\"version\":\"1\",\"totalAdPoint\":78900,\"message\":\"みちのくディルドさんが500ptニコニ広告しました「お婆さんが浮いてるぞ！気を付けろ！」\"}</chat>");

            var userMock = new Mock<IUser>();
            var user = userMock.Object;

            var serverMock = new Mock<IDataSource>();
            var server = serverMock.Object;

            var loggerMock = new Mock<ILogger>();
            var logger = loggerMock.Object;

            var siteOptionsMock = new Mock<INicoSiteOptions>();
            var siteOptions = siteOptionsMock.Object;

            var message = Tools.CreateNicoAd(chat, "roomname", siteOptions);
            Assert.IsNotNull(message);
            Assert.AreEqual("みちのくディルドさんが500ptニコニ広告しました「お婆さんが浮いてるぞ！気を付けろ！」", message.Text);
            Assert.AreEqual(NicoMessageType.Ad, message.NicoMessageType);
        }
        [Test]
        public async Task ParseCommentTest()
        {
            var chat = Chat.Parse("<chat thread=\"1649512884\" no=\"7071\" vpos=\"1982200\" date=\"1556821824\" date_usec=\"556684\" mail=\"184\" user_id=\"6nf21kHUbAcbMD6aTNy_yu5WqdM\" anonymity=\"1\" locale=\"ja-jp\">返事適当じゃない？</chat>");

            var userMock = new Mock<IUser>();
            var user = userMock.Object;

            var serverMock = new Mock<IDataSource>();
            var server = serverMock.Object;

            var loggerMock = new Mock<ILogger>();
            var logger = loggerMock.Object;

            var siteOptionsMock = new Mock<INicoSiteOptions>();
            var siteOptions = siteOptionsMock.Object;

            var message = await Tools.CreateNicoComment(chat, user, siteOptions, "roomname", async userid => await API.GetUserInfo(server, userid), logger) as INicoComment;
            Assert.IsNotNull(message);
            Assert.AreEqual(7071, message.ChatNo);
            Assert.AreEqual("返事適当じゃない？", message.Text);
            Assert.AreEqual("roomname:7071", message.Id);
            Assert.IsTrue(message.Is184);
            Assert.AreEqual(SiteType.NicoLive, message.SiteType);
            Assert.AreEqual("6nf21kHUbAcbMD6aTNy_yu5WqdM", message.UserId);

        }
    }
}
