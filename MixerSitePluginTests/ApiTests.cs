using MixerSitePlugin;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using System.Net;

namespace MixerSitePluginTests
{
    [TestFixture]
    class ApiTests
    {
        [Test]
        public async Task GetCurrentUserInfoTest()
        {
            var data = TestHelper.GetSampleData("users_current.txt");
            var serverMock = new Mock<IDataServer>();
            serverMock.Setup(s => s.GetWithNoThrowAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CookieContainer>())).Returns(Task.FromResult(data));
            var server = serverMock.Object;
            var user = await Api.GetCurrentUserInfo(server, new CookieContainer()) as CurrentUser;
            Assert.AreEqual(714570, user.Id);
            Assert.AreEqual(59088, user.Experience);
            Assert.AreEqual(82, user.Level);
            Assert.AreEqual(1324484, user.Sparks);

        }
        [Test]
        public async Task GetChatInfoTest()
        {
            var data = TestHelper.GetSampleData("chats.txt");
            var serverMock = new Mock<IDataServer>();
            serverMock.Setup(s => s.GetAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CookieContainer>())).Returns(Task.FromResult(data));
            var server = serverMock.Object;
            var chatInfo = await Api.GetChatInfo(server, 123456, new CookieContainer());
            Assert.AreEqual("XJ3EbJfx2lioJU0v", chatInfo.Authkey);
            Assert.AreEqual(new[] { "wss://chat.mixer.com:443" }, chatInfo.Endpoints);
            Assert.IsFalse(chatInfo.IsLoadShed);
            Assert.AreEqual(new[] { "chat", "connect", "poll_vote", "whisper" }, chatInfo.Permissions);
            Assert.AreEqual(new[] { "User" }, chatInfo.Roles);
        }
        [Test]
        public async Task GetChannelInfoTest()
        {
            var data = TestHelper.GetSampleData("channels.txt");
            var serverMock = new Mock<IDataServer>();
            serverMock.Setup(s => s.GetAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CookieContainer>())).Returns(Task.FromResult(data));
            var server = serverMock.Object;
            var channelInfo = await Api.GetChannelInfo(server, "CHANNEL_NAME", new CookieContainer());
            Assert.AreEqual(160788, channelInfo.Id);
        }
    }
}
