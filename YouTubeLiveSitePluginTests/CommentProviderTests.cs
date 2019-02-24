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

namespace YouTubeLiveSitePluginTests
{
    [TestFixture]
    internal class CommentProviderTests
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
        class C: CommentProvider
        {
            public C(ICommentOptions options, IYouTubeLibeServer server, YouTubeLiveSiteOptions siteOptions, ILogger logger, IUserStore userStore, string clientIdPrefix, string sej, string sessionToken)
                :base(options,server,siteOptions,logger,userStore)
            {
                PostCommentContext = new PostCommentContext
                {
                     ClientIdPrefix = clientIdPrefix,
                      Sej=sej,
                       SessionToken=sessionToken,
                };
            }
        }
        [Test]
        public async Task PostTest()
        {
            var comment = "あいうえお";
            var clientIdPrefix = "a";
            var sej = "b";
            var sessionToken = "c";
            var options = new Mock<ICommentOptions>();
            var server = new Server(clientIdPrefix, comment,sej, sessionToken);
            var siteOptions = new YouTubeLiveSiteOptions();
            var logger = new Mock<ILogger>();
            var userStore = new Mock<IUserStore>();

            var cp = new C(options.Object, server, siteOptions, logger.Object, userStore.Object, clientIdPrefix, sej, sessionToken);
            
            //TODOちゃんとtestという文字列が投稿されるかテストする
            await cp.PostCommentAsync(comment);
        }
        [Test]
        public async Task ConnectedEventTest()
        {
            var options = new Mock<ICommentOptions>();
            var serverMock = new Mock<IYouTubeLibeServer>();
            serverMock.Setup(s => s.GetEnAsync("https://www.youtube.com/channel/UCv1fFr156jc65EMiLbaLImw/videos?flow=list&view=2")).Returns(Task.FromResult(Tools.GetSampleData("Channel_on_air.txt")));
            serverMock.Setup(s => s.GetAsync("https://www.youtube.com/live_chat?v=AuFOOUtIyUY&is_popout=1",It.Is<CookieContainer>(c=>true))).Returns(Task.FromResult(Tools.GetSampleData("LiveChat.txt")));
            var siteOptions = new YouTubeLiveSiteOptions();
            var logger = new Mock<ILogger>();
            var userStore = new Mock<IUserStore>();
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
            var userStore = new Mock<IUserStore>();
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
            var userStore = new Mock<IUserStore>();
            var broweserProfileMock = new Mock<IBrowserProfile>();
            var cp = new CommentProvider(options.Object, serverMock.Object, siteOptions, loggerMock.Object, userStore.Object);
            var info = await cp.GetCurrentUserInfo(broweserProfileMock.Object);
            Assert.IsFalse(info.IsLoggedIn);
        }
        [Test]
        public async Task 短すぎるコメントを投稿したときのエラーメッセージを正しく処理できるか()
        {
            var data = Tools.GetSampleData("CommentPost_Result_TooShort.txt");
            var options = new Mock<ICommentOptions>();
            var serverMock = new Mock<IYouTubeLibeServer>();
            serverMock.Setup(s => s.PostAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CookieContainer>())).Returns(Task.FromResult(data));
            var siteOptions = new YouTubeLiveSiteOptions();
            var loggerMock = new Mock<ILogger>();
            var userStore = new Mock<IUserStore>();
            var broweserProfileMock = new Mock<IBrowserProfile>();
            var cpMock = new Mock<CommentProvider>(options.Object, serverMock.Object, siteOptions, loggerMock.Object, userStore.Object);
            cpMock.Protected().Setup<PostCommentContext>("PostCommentContext").Returns(new PostCommentContext() { Sej = "" });
            var cp = cpMock.Object;
            bool expectedResult = false;
            cp.CommentReceived += (s, e) =>
            {
                if ((e.MessageItems.ToList()[0] as IMessageText).Text == "コメント投稿に失敗しました（コメントが短すぎます。）")
                {
                    expectedResult = true;
                }
            };
            await cp.PostCommentAsync("");
            Assert.IsTrue(expectedResult);
        }
    }
}
