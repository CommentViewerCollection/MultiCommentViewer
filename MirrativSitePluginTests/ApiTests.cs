using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MirrativSitePlugin;
using Moq;
using SitePlugin;
using Common;
using ryu_s.BrowserCookie;
using System.Net;

namespace MirrativSitePluginTests
{
    [TestFixture]
    public class ApiTests
    {
        [Test]
        public async Task GetCurrentUserAsync_LoggedInDataTest()
        {
            var data = Tools.GetSampleData("Home_loggedin.txt");
            var serverMock = new Mock<IDataServer>();
            serverMock.Setup(s => s.GetAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CookieContainer>())).Returns(Task.FromResult(data));
            var currentUser = await Api.GetCurrentUserAsync(serverMock.Object, null);
            Assert.AreEqual("4566407", currentUser.UserId);
            Assert.AreEqual("Ryu", currentUser.Name);
        }
        [Test]
        public async Task GetCurrentUserAsync_NotLoggedInDataTest()
        {
            var data = Tools.GetSampleData("Home_not_loggedin.txt");
            var serverMock = new Mock<IDataServer>();
            serverMock.Setup(s => s.GetAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CookieContainer>())).Returns(Task.FromResult(data));
            var currentUser = await Api.GetCurrentUserAsync(serverMock.Object, null);
            Assert.IsFalse(currentUser.IsLoggedIn);
        }
        [Test]
        public async Task GetLiveInfoTest()
        {
            var liveId = "abc";
            var url = "https://www.mirrativ.com/api/live/live?live_id=" + liveId;
            var data = Tools.GetSampleData("LiveInfo.txt");
            var serverMock = new Mock<IDataServer>();
            serverMock.Setup(s => s.GetAsync(url, It.IsAny<Dictionary<string, string>>())).Returns(Task.FromResult(data));
            var server = serverMock.Object;
            var ret = await Api.GetLiveInfo(server, liveId);
            Assert.AreEqual("PUBG全力参加待ち(*´ー｀*)初見さんつかまえる #ハロウィンガチャ", ret.Title);
            Assert.AreEqual("118f91f:UdoBre1M", ret.Broadcastkey);
        }
        //[Test]
        //public async Task Test2()
        //{
        //    var url = "Fnp3oyQKryxnSZDpWD6nrA";
        //    var browserMock = new Mock<IBrowserProfile>();
        //    var optionsMock = new Mock<ICommentOptions>();
        //    var options = optionsMock.Object;
        //    var loggerMock = new Mock<ILogger>();
        //    var logger = loggerMock.Object;
        //    var browser = browserMock.Object;
        //    var context = new MirrativSiteContext(options, new TwitchServer(), logger);
        //    var cp = context.CreateCommentProvider();
        //    await cp.ConnectAsync(url, browser);
        //}
    }
}
