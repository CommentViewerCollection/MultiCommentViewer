using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using YouTubeLiveSitePlugin.Test2;
using Moq;
using Moq.Protected;
using SitePlugin;
using Common;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Net.Http;
using ryu_s.BrowserCookie;
using YouTubeLiveSitePlugin;
using SitePluginCommon;
using System.Diagnostics;

namespace YouTubeLiveSitePluginTests
{
    [TestFixture]
    public class CommentProviderTests
    {

        public class Server : IYouTubeLibeServer
        {
            private readonly string _clientIdPrefix;
            private readonly string _comment;
            private readonly string _sej;
            private readonly string _sessionToken;

            public Task<string> GetAsync(string url)
            {
                throw new NotImplementedException();
            }

            public Task<string> GetEnAsync(string url)
            {
                throw new NotImplementedException();
            }

            public Task<string> PostAsync(string url, string data, CookieContainer cc)
            {
                //client_message_id=a0&rich_message=%7B%22text_segments%22%3A%5B%7B%22text%22%3A%22test%22%7D%5D%7D&sej=b&session_token=c
                {
                    var match = Regex.Match(data, "rich_message=([^&]+)");
                    if (!match.Success) Assert.Fail();
                    var s = match.Groups[1].Value;
                    var decoded = HttpUtility.UrlDecode(s);
                    Assert.AreEqual("{\"text_segments\":[{\"text\":\"" + _comment + "\"}]}", decoded);
                }
                {
                    var match = Regex.Match(data, "client_message_id=([^&]+)");
                    if (!match.Success) Assert.Fail();
                    var s = match.Groups[1].Value;
                    var decoded = HttpUtility.UrlDecode(s);
                    Assert.AreEqual(_clientIdPrefix + "0", decoded);
                }
                {
                    var match = Regex.Match(data, "sej=([^&]+)");
                    if (!match.Success) Assert.Fail();
                    var s = match.Groups[1].Value;
                    var decoded = HttpUtility.UrlDecode(s);
                    Assert.AreEqual(_sej, decoded);
                }
                {
                    var match = Regex.Match(data, "session_token=([^&]+)");
                    if (!match.Success) Assert.Fail();
                    var s = match.Groups[1].Value;
                    var decoded = HttpUtility.UrlDecode(s);
                    Assert.AreEqual(_sessionToken, decoded);
                }
                return Task.FromResult("");
            }

            public Task<string> GetAsync(string url, CookieContainer cc)
            {
                throw new NotImplementedException();
            }

            public Task<string> PostAsync(string url, Dictionary<string, string> data, CookieContainer cc)
            {
                throw new NotImplementedException();
            }

            public Task<string> PostAsync(HttpOptions options, HttpContent content)
            {
                throw new NotImplementedException();
            }

            public Task<byte[]> GetBytesAsync(string url)
            {
                throw new NotImplementedException();
            }

            public Server(string clientIdPrefix, string comment, string sej, string sessionToken)
            {
                _clientIdPrefix = clientIdPrefix;
                _comment = comment;
                _sej = sej;
                _sessionToken = sessionToken;
            }
        }
        [Test]
        public async Task PostTest()
        {
            var comment = "あいうえお";
            var clientIdPrefix = "a";
            var sej = "b";
            var sessionToken = "c";
            var cc = new CookieContainer();
            var optionsMock = new Mock<ICommentOptions>();
            var options = optionsMock.Object;
            var server = new Server(clientIdPrefix, comment, sej, sessionToken);
            var serverMock = new Mock<IYouTubeLibeServer>();
            //serverMock.Setup(s => s.PostAsync(It.IsAny<string>(), It.IsAny<Dictionary<string,string>>(), It.IsAny<CookieContainer>())).Callback<Task<string>>(a=>PostAsync(a,b,c));
            //serverMock.Verify(h=>h.PostAsync())
            var siteOptions = new YouTubeLiveSiteOptions();
            var loggerMock = new Mock<ILogger>();
            var logger = loggerMock.Object;
            var userStoreMock = new Mock<IUserStoreManager>();
            var userStoreManager = userStoreMock.Object;
            var userCommentCountDict = new Dictionary<string, int>();
            var receivedCommentIds = new SynchronizedCollection<string>();
            var cpMock = CreateCommentProviderMock(options, server, siteOptions, logger, userStoreManager);
            var cp = cpMock.Object;
            //var cp = new C(options.Object, server, siteOptions, logger.Object, userStore.Object, clientIdPrefix, sej, sessionToken);
            var connMock = CreateConnection(logger, cc, options, server, siteOptions, userCommentCountDict, receivedCommentIds, cp, userStoreManager, Guid.NewGuid());
            connMock.Protected().Setup<PostCommentContext>("PostCommentContext").Returns(new PostCommentContext
            {
                ClientIdPrefix = clientIdPrefix,
                Sej = sej,
                SessionToken = sessionToken,
            });
            var connection = connMock.Object;
            //TODOちゃんとtestという文字列が投稿されるかテストする
            await connection.PostCommentAsync(comment);

            connMock.VerifyAll();
        }

        private static Mock<CommentProvider> CreateCommentProviderMock(ICommentOptions options, IYouTubeLibeServer server, YouTubeLiveSiteOptions siteOptions, ILogger logger, IUserStoreManager userStoreManager)
        {
            return new Mock<CommentProvider>(options, server, siteOptions, logger, userStoreManager);
        }

        [Test]
        public async Task ConnectedEventTest()
        {
            var options = new Mock<ICommentOptions>();
            var serverMock = new Mock<IYouTubeLibeServer>();
            serverMock.Setup(s => s.GetEnAsync("https://www.youtube.com/channel/UCv1fFr156jc65EMiLbaLImw/live")).Returns(Task.FromResult(Tools.GetSampleData("Channel_live.txt")));
            serverMock.Setup(s => s.GetAsync("https://www.youtube.com/live_chat?v=klvzbBP7zM8&is_popout=1", It.Is<CookieContainer>(c=>true))).Returns(Task.FromResult(Tools.GetSampleData("LiveChat.txt")));
            var siteOptions = new YouTubeLiveSiteOptions();
            var logger = new Mock<ILogger>();
            var userStore = new Mock<IUserStoreManager>();
            var broweserProfileMock = new Mock<IBrowserProfile>();

            var b = false;
            var cp = new CommentProvider(options.Object, serverMock.Object, siteOptions, logger.Object, userStore.Object);
            cp.Connected += (s, e) =>
            {
                b = e.IsInputStoringNeeded;
            };
            await cp.ConnectAsync("https://www.youtube.com/channel/UCv1fFr156jc65EMiLbaLImw", broweserProfileMock.Object);
            Assert.IsTrue(b);
        }
        [Test]
        public async Task GetCurrentUserInfoAsync_LoggedInTest()
        {
            var data = Tools.GetSampleData("Embed_loggedin.txt");
            var options = new Mock<ICommentOptions>();
            var serverMock = new Mock<IYouTubeLibeServer>();
            serverMock.Setup(s => s.GetAsync(It.IsAny<string>(), It.IsAny<CookieContainer>())).Returns(Task.FromResult(data));
            var siteOptions = new YouTubeLiveSiteOptions();
            var loggerMock = new Mock<ILogger>();
            var userStore = new Mock<IUserStoreManager>();
            var broweserProfileMock = new Mock<IBrowserProfile>();
            var cp = new CommentProvider(options.Object, serverMock.Object, siteOptions, loggerMock.Object, userStore.Object);
            var info = await cp.GetCurrentUserInfo(broweserProfileMock.Object);
            Assert.IsTrue(info.IsLoggedIn);
            Assert.AreEqual("Ryu", info.Username);
        }
        [Test]
        public async Task GetCurrentUserInfoAsync_NotLoggedInTest()
        {
            var data = Tools.GetSampleData("Embed_notloggedin.txt");
            var options = new Mock<ICommentOptions>();
            var serverMock = new Mock<IYouTubeLibeServer>();
            serverMock.Setup(s => s.GetAsync(It.IsAny<string>(), It.IsAny<CookieContainer>())).Returns(Task.FromResult(data));
            var siteOptions = new YouTubeLiveSiteOptions();
            var loggerMock = new Mock<ILogger>();
            var userStore = new Mock<IUserStoreManager>();
            var broweserProfileMock = new Mock<IBrowserProfile>();
            var cp = new CommentProvider(options.Object, serverMock.Object, siteOptions, loggerMock.Object, userStore.Object);
            var info = await cp.GetCurrentUserInfo(broweserProfileMock.Object);
            Assert.IsFalse(info.IsLoggedIn);
        }
        [Test]
        public async Task 短すぎるコメントを投稿したときのエラーメッセージを正しく処理できるか()
        {
            var data = Tools.GetSampleData("CommentPost_Result_TooShort.txt");
            var optionsMock = new Mock<ICommentOptions>();
            var serverMock = new Mock<IYouTubeLibeServer>();
            serverMock.Setup(s => s.PostAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CookieContainer>())).Returns(Task.FromResult(data));
            var siteOptions = new YouTubeLiveSiteOptions();
            var loggerMock = new Mock<ILogger>();
            var userStoreManagerMock = new Mock<IUserStoreManager>();
            var broweserProfileMock = new Mock<IBrowserProfile>();
            var cpMock = new Mock<EachConnection>(loggerMock.Object, new CookieContainer(), optionsMock.Object, serverMock.Object, siteOptions, new Dictionary<string, int>(), new System.Collections.Generic.SynchronizedCollection<string>(), new Mock<ICommentProvider>().Object, userStoreManagerMock.Object);
            //var cpMock = new Mock<EachConnection>(options.Object, serverMock.Object, siteOptions, loggerMock.Object, userStore.Object);
            cpMock.Protected().Setup<PostCommentContext>("PostCommentContext").Returns(new PostCommentContext() { Sej = "" });
            var cp = cpMock.Object;
            bool expectedResult = false;
            cp.MessageReceived += (s, e) =>
            {
                var message = e.Message as IInfoMessage;
                if ((message.CommentItems.ToList()[0] as IMessageText).Text == "コメント投稿に失敗しました（コメントが短すぎます。）")
                {
                    expectedResult = true;
                }
            };
            await cp.PostCommentAsync("");
            Assert.IsTrue(expectedResult);
        }
        private Mock<EachConnection> CreateConnection(ILogger logger, CookieContainer cc, ICommentOptions options, IYouTubeLibeServer server,
            YouTubeLiveSiteOptions siteOptions, Dictionary<string, int> userCommentCountDict, SynchronizedCollection<string> receivedCommentIds,
            ICommentProvider cp, IUserStoreManager userStoreManager, Guid siteContextGuid)
        {
            var cpMock = new Mock<EachConnection>(logger, cc, options, server, siteOptions, userCommentCountDict, receivedCommentIds, cp, userStoreManager) { CallBase = true };
            cpMock.Object.SiteContextGuid = siteContextGuid;
            return cpMock;
        }
        [Test]
        public async Task 再接続時の初期コメントが重複判定されるか()
        {
            var optionsMock = new Mock<ICommentOptions>();
            var options = optionsMock.Object;
            var serverMock = new Mock<IYouTubeLibeServer>();
            serverMock.Setup(s => s.GetAsync("https://www.youtube.com/live_chat?v=EiLzFNajLas&is_popout=1", It.Is<CookieContainer>(c => true))).Returns(Task.FromResult(Tools.GetSampleData("LiveChat.txt")));
            serverMock.Setup(s => s.GetBytesAsync("https://www.youtube.com/live_chat/get_live_chat?continuation=0ofMyAPiARqQAUNpTVNJUW9ZVlVOSGFsWTBZbk5ETkROUGJpMVpkV2xNV21ObVREQjNFZ1V2YkdsMlpScERxcm5CdlFFOUNqdG9kSFJ3Y3pvdkwzZDNkeTU1YjNWMGRXSmxMbU52YlM5c2FYWmxYMk5vWVhRX2RqMUJkVVpQVDFWMFNYbFZXU1pwYzE5d2IzQnZkWFE5TVNBQyiR8P7MuYjcAjAAOABAAUogCAAQABgAIAAqBzY1ZTg4M2Y6AEAASgBQ78C90LmI3AJQoIPEzbmI3AJYgdDVjPqH3AJoBIIBBAgEEACIAQA%253D&pbj=1")).ThrowsAsync(new HttpRequestException());
            var server = serverMock.Object;

            var siteOptions = new YouTubeLiveSiteOptions();
            var loggerMock = new Mock<ILogger>();
            var logger = loggerMock.Object;
            var userStoreManagerMock = new Mock<IUserStoreManager>();
            userStoreManagerMock.Setup(u => u.GetUser(SiteType.YouTubeLive, It.IsAny<string>())).Returns(new UserTest("1"));
            var userStore = userStoreManagerMock.Object;
            var browserProfileMock = new Mock<IBrowserProfile>();
            browserProfileMock.Setup(b => b.GetCookieCollection(It.IsAny<string>())).Returns(new List<Cookie>());
            var browserProfile = browserProfileMock.Object;

            var userCommentCountDict = new Dictionary<string, int>();
            var receivedCommentIds = new SynchronizedCollection<string>();
            var cpMock = new Mock<CommentProvider>(options, server, siteOptions, logger, userStore) { CallBase = true };
            var cp = cpMock.Object;
            var eachConnectionMock = CreateConnection(logger, new CookieContainer(), options, server, siteOptions, userCommentCountDict, receivedCommentIds, cp, userStore, Guid.NewGuid());
            eachConnectionMock.Protected().Setup<Task>("CreateMetadataReceivingTask", ItExpr.Ref<IMetadataProvider>.IsAny, ItExpr.IsAny<BrowserType>(), ItExpr.IsAny<string>(), ItExpr.IsAny<string>()).Returns(Task.CompletedTask);
            cpMock.Protected().Setup<EachConnection>("CreateConnection",
                ItExpr.IsAny<ILogger>(),
                ItExpr.IsAny<CookieContainer>(),
                ItExpr.IsAny<ICommentOptions>(),
                ItExpr.IsAny<IYouTubeLibeServer>(),
                ItExpr.IsAny<YouTubeLiveSiteOptions>(),
                ItExpr.IsAny<Dictionary<string, int>>(),
                ItExpr.IsAny<SynchronizedCollection<string>>(),
                ItExpr.IsAny<ICommentProvider>(),
                ItExpr.IsAny<IUserStoreManager>(),
                ItExpr.IsAny<Guid>()
                ).Returns(eachConnectionMock.Object);
            int i = 0;
            int j = 0;
            cp.MessageReceived += (s, e) =>
            {
                if(e.Message is YouTubeLiveComment)
                {
                    i++;
                }
                else if(e.Message is IInfoMessage info && info.Type <= InfoType.Error)
                {
                    Debug.WriteLine(info.CommentItems.ToText());
                    j++;
                }
                if(i > 75)
                {
                    Assert.Fail();
                }
                if(j >= 2)
                {
                    cp.Disconnect();
                }
            };
            await cp.ConnectAsync("https://www.youtube.com/watch?v=EiLzFNajLas", browserProfile);
            Assert.AreEqual(75, i);
            Assert.AreEqual(2, j);
        }
    }
}
