using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Moq;
using OpenrecSitePlugin;
using System.Net;
namespace OpenrecSitePluginTests
{
    [TestFixture]
    class APITests
    {
        [Test]
        public async Task GetBlackListAsyncTest()
        {
            var data = DataLoader.GetSampleData(@"blacklists.txt");
            var serverMock = new Mock<IDataSource>();
            serverMock.Setup(s => s.GetAsync(It.IsAny<string>(), It.IsAny<Dictionary<string,string>>())).ReturnsAsync(data);
            var context = new Context("", "");
            var blacklist = await API.GetBanList(serverMock.Object, context);

        }
        [Test]
        public async Task GetMeAsync_LoggedIn_Test()
        {
            var data = DataLoader.GetSampleData(@"Home_loggedin.txt");
            var serverMock = new Mock<IDataSource>();
            serverMock.Setup(s => s.GetAsync(It.IsAny<string>(), It.IsAny<CookieContainer>())).ReturnsAsync(data);
            var me = await API.GetMeAsync(serverMock.Object, new CookieContainer());
            Assert.AreEqual("たこやき", me.DisplayName);
            Assert.AreEqual("kv510k", me.UserPath);
        }
        [Test]
        public async Task GetMeAsync_NotLoggedIn_Test()
        {
            var data = DataLoader.GetSampleData(@"Home_notloggedin.txt");
            var serverMock = new Mock<IDataSource>();
            serverMock.Setup(s => s.GetAsync(It.IsAny<string>(), It.IsAny<CookieContainer>())).ReturnsAsync(data);
            var me = await API.GetMeAsync(serverMock.Object, new CookieContainer());
            Assert.IsTrue(string.IsNullOrEmpty(me.DisplayName));
            Assert.IsTrue(string.IsNullOrEmpty(me.UserPath));
        }
        [Test]
        public async Task GetMovieInfoTests()
        {
            var liveId = "abc";
            var data = DataLoader.GetSampleData(@"External\Movies\TextFile1.txt");
            var serverMock = new Mock<IDataSource>();
            serverMock.Setup(s => s.GetAsync("https://public.openrec.tv/external/api/v5/movies/" + liveId, It.IsAny<CookieContainer>())).ReturnsAsync(data);
            var info = await API.GetMovieInfo(serverMock.Object, liveId, new CookieContainer());
            Assert.AreEqual("FkJTQSjkQps", info.Id);
            Assert.AreEqual(993793, info.MovieId);
        }
    }
}
