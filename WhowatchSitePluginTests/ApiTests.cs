using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WhowatchSitePlugin;

namespace WhowatchSitePluginTests
{
    [TestFixture]
    class ApiTests
    {
        [Test]
        public async Task GetLiveDataTest()
        {
            var live_id = 123;
            var lastUpdatedAt = 0;
            var data = DataLoader.GetSampleData("LiveData.txt");
            var serverMock = new Mock<IDataServer>();
            serverMock.Setup(s => s.GetAsync(It.IsAny<string>(), It.IsAny<CookieContainer>())).Returns(Task.FromResult(data));
            var cc = new CookieContainer();
            var liveData = await Api.GetLiveDataAsync(serverMock.Object, live_id, lastUpdatedAt, cc);
            Assert.AreEqual("👼🏻こっちん様🎀のライブ", liveData.Live.Title);
            Assert.AreEqual(1532194251671, liveData.UpdatedAt);
            Assert.AreEqual(413277878, liveData.Comments[0].Id);
            Assert.AreEqual(1003, liveData.Comments[0].User.Id);
            Assert.AreEqual("匿名係長ただの花火師", liveData.Comments[0].User.Name);
        }
        [Test]
        public async Task GetProfileTest()
        {
            var userPath = "w:abc";
            var data = DataLoader.GetSampleData("Profile.txt");
            var serverMock = new Mock<IDataServer>();
            serverMock.Setup(s => s.GetAsync("https://api.whowatch.tv/users/" + userPath + "/profile", It.IsAny<CookieContainer>())).Returns(Task.FromResult(data));
            var cc = new CookieContainer();
            var profile = await Api.GetProfileAsync(serverMock.Object, userPath, cc);
            Assert.AreEqual("👼🏻こっちん様🎀", profile.Name);
            Assert.AreEqual("ふ:koto0316", profile.AccountName);
            Assert.AreEqual(7005919, profile.Live.Id);
            Assert.AreEqual("👼🏻こっちん様🎀のライブ", profile.Live.Title);
            Assert.AreEqual(1532189547000, profile.Live.StartedAt);
        }
        [Test]
        public async Task MeTest()
        {
            var ret = "{\"id\":1072838,\"user_type\":\"VALID\",\"user_code\":\"1778649641148661\",\"account_register_status\":\"TWITTER\",\"whowatch_point\":0,\"icon_url\":\"\",\"account_name\":\"@kv510k\",\"user_path\":\"t:kv510k\",\"name\":\"Ryu\",\"is_email_registered\":false,\"is_twitter_connected\":true,\"is_facebook_connected\":false,\"is_related_account_auto_blocked\":false}";
            var serverMock = new Mock<IDataServer>();
            serverMock.Setup(s => s.GetAsync("https://api.whowatch.tv/users/me", It.IsAny<CookieContainer>())).Returns(Task.FromResult(ret));
            var cc = new CookieContainer();
            var me = await Api.GetMeAsync(serverMock.Object, cc);
            Assert.AreEqual("@kv510k", me.AccountName);
            Assert.AreEqual("t:kv510k", me.UserPath);
            Assert.AreEqual("Ryu", me.Name);
        }
        [Test]
        public async Task GetPlayItems()
        {
            var data = DataLoader.GetSampleData("PlayItems.txt");
            var serverMock = new Mock<IDataServer>();
            var server = serverMock.Object;
            serverMock.Setup(s => s.GetAsync("https://api.whowatch.tv/playitems")).Returns(Task.FromResult(data));
            var dict = await Api.GetPlayItemsAsync(server);
            var item = dict[182];
            Assert.AreEqual("大花火", item.Name);
        }
    }
}
