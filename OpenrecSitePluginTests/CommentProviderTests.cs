using Common;
using Moq;
using NUnit.Framework;
using OpenrecSitePlugin;
using ryu_s.BrowserCookie;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Moq.Protected;
using System.Threading;

namespace OpenrecSitePluginTests
{
    [TestFixture]
    class CommentProviderTests
    {
        class Websocket : IOpenrecWebsocket
        {
            CancellationTokenSource _cts;
            public event EventHandler<IPacket> Received;

            public void Disconnect()
            {
                _cts?.Cancel();
            }

            public async Task ReceiveAsync(string movieId, string userAgent, List<Cookie> cookies)
            {
                _cts = new CancellationTokenSource();
                while (!_cts.IsCancellationRequested)
                {
                    await Task.Delay(100);
                }
            }

            public Task SendAsync(IPacket packet)
            {
                throw new NotImplementedException();
            }

            public Task SendAsync(string s)
            {
                throw new NotImplementedException();
            }
            public void OnReceived(string data)
            {
                Received?.Invoke(this, Packet.Parse(data));
            }
        }
        [Test]
        public async Task CommentReceiveTest()
        {
            var guid = Guid.NewGuid();
            var optionsMock = new Mock<ICommentOptions>();
            var siteOptionsMock = new Mock<OpenrecSiteOptions>();
            var loggerMock = new Mock<ILogger>();
            var userMock = new Mock<IUser2>();
            userMock.Setup(u => u.UserId).Returns("123");
            var userStoreMock = new Mock<IUserStore>();
            userStoreMock.Setup(u => u.GetUser(It.IsAny<string>())).Returns(userMock.Object);
            var browserProfileMock = new Mock<IBrowserProfile>();
            var ws = new Websocket();

            var bpMock = new Mock<IBlackListProvider>();

            var options = optionsMock.Object;
            var siteOptions = siteOptionsMock.Object;
            var logger = loggerMock.Object;
            var userStore = userStoreMock.Object;
            var browserProfile = browserProfileMock.Object;
            var movieInfo1 = new MovieInfo
            {
                MovieId = 1,
            };

            var cpMock = new Mock<CommentProvider>(options, siteOptions, logger, userStore);
            cpMock.Protected().Setup<List<Cookie>>("GetCookies", ItExpr.IsAny<IBrowserProfile>()).Returns(new List<Cookie>
            {
                 new Cookie("uuid","abc","/","a"),
                 new Cookie("access_token","abc","/","a"),
            });
            cpMock.Protected().Setup<Task<MovieInfo>>("GetMovieInfo", ItExpr.IsAny<string>()).Returns(Task.FromResult(movieInfo1));
            cpMock.Protected().Setup<IOpenrecWebsocket>("CreateOpenrecWebsocket").Returns(ws);
            cpMock.Protected().Setup<IBlackListProvider>("CreateBlacklistProvider").Returns(bpMock.Object);

            var cp = cpMock.Object;
            cp.SiteContextGuid = guid;
            var received = false;
            cp.MessageReceived += (s, e) =>
            {
                if (e.Message is IOpenrecComment comment)
                {
                    var metadata = e.Metadata;
                    
                    Assert.AreEqual(new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText("そっくりだな") }, comment.CommentItems);
                    Assert.AreEqual("258587691", comment.Id);
                    Assert.AreEqual(new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText("il") }, comment.NameItems);
                    Assert.AreEqual(OpenrecMessageType.Comment, comment.OpenrecMessageType);
                    Assert.AreEqual("16:13:06", comment.PostTime);
                    //Assert.IsTrue(!string.IsNullOrEmpty(comment.Raw));
                    Assert.AreEqual(SiteType.Openrec, comment.SiteType);
                    Assert.IsNull(comment.UserIcon);
                    Assert.AreEqual("330854891", comment.UserId);

                    Assert.AreEqual(guid, metadata.SiteContextGuid);

                    ws.Disconnect();
                    received = true;
                }
            };
            var t = cp.ConnectAsync("", browserProfile);
            //コメントとExpectedをセットするだけでテストが書けるようにしたい
            var data1 = "42[\"message\",\"{\\\"type\\\":0,\\\"data\\\":{\\\"movie_id\\\":1257976,\\\"live_type\\\":1,\\\"onair_status\\\":1,\\\"user_id\\\":330854891,\\\"openrec_user_id\\\":1695348,\\\"user_name\\\":\\\"il\\\",\\\"user_type\\\":\\\"2\\\",\\\"user_key\\\":\\\"tropicalgorilla\\\",\\\"user_rank\\\":0,\\\"chat_id\\\":258587691,\\\"item\\\":0,\\\"golds\\\":0,\\\"message\\\":\\\"そっくりだな\\\",\\\"cre_dt\\\":\\\"2019-04-21 16:13:06\\\",\\\"is_fresh\\\":0,\\\"is_warned\\\":0,\\\"is_moderator\\\":0,\\\"is_premium\\\":0,\\\"has_banned_word\\\":0,\\\"stamp\\\":null,\\\"quality_type\\\":0,\\\"user_icon\\\":\\\"https://openrec-appdata.s3.amazonaws.com/user/3308549/330854891.png?1555827558\\\",\\\"supporter_rank\\\":0,\\\"is_creaters\\\":0,\\\"is_premium_hidden\\\":0,\\\"user_color\\\":\\\"#F6A434\\\",\\\"yell\\\":null,\\\"yell_type\\\":null,\\\"to_user\\\":null,\\\"capture\\\":null,\\\"display_dt\\\":\\\"0秒前\\\",\\\"del_flg\\\":0}}\"]";
            ws.OnReceived(data1);
            await t;
            Assert.IsTrue(received);
        }
    }
}
