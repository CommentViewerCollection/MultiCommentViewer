using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Common;
using Moq;
using NUnit.Framework;
using ryu_s.BrowserCookie;
using SitePlugin;
using SitePluginCommon;
using TwitchSitePlugin;

namespace TwitchSitePluginTests
{
    [TestFixture]
    class TwitchCommentProviderTests
    {
        class C : TwitchCommentProvider
        {
            public bool IsLoggedin { get; set; }
            protected override bool IsLoggedIn()
            {
                return IsLoggedin;
            }
            protected override string GetChannelName(string input)
            {
                return "";
            }
            protected override CookieContainer GetCookieContainer(IBrowserProfile browserProfile)
            {
                return new CookieContainer();
            }
            protected override string GetRandomGuestUsername()
            {
                return "";
            }
            public IMessageProvider MessageProvider { get; set; }
            protected override IMessageProvider CreateMessageProvider()
            {
                return MessageProvider;
            }
            public IMetadataProvider MetadataProvider { get; set; }
            protected override IMetadataProvider CreateMetadataProvider(string channelName)
            {
                return MetadataProvider;
            }
            public ICommentData CommentData { get; set; }
            //protected override ICommentData ParsePrivMsg(Result result)
            //{
            //    return CommentData;
            //}
            public C(IDataServer server, ILogger logger, ICommentOptions options, TwitchSiteOptions siteOptions, IUserStoreManager userStoreManager) 
                : base(server, logger, options, siteOptions, userStoreManager)
            {
            }
        }
        class MessageProvider : IMessageProvider
        {
            public event EventHandler<Result> Received;
            public event EventHandler Opened;
            TaskCompletionSource<object> _tcs = new TaskCompletionSource<object>();
            public void Disconnect()
            {
                _tcs.SetResult(null);
            }

            public Task ReceiveAsync()
            {
                return _tcs.Task;
            }

            public Task SendAsync(string s)
            {
                throw new NotImplementedException();
            }
            public void SetResult(Result result)
            {
                Received?.Invoke(this, result);
            }
        }
        class MetadataProvider : IMetadataProvider
        {
            public event EventHandler<Stream> MetadataUpdated;

            public void Disconnect()
            {
            }

            public Task ReceiveAsync()
            {
                return Task.CompletedTask;
            }
        }
        [Test]
        public async Task Test()
        {
            //テスト案
            //ログイン済み、未ログインそれぞれの場合にそれぞれの接続コマンドが送信されるか
            //サーバから送られてくるコマンドに対する反応は適切か。PINGの時はPONGが返されるか、PRIVMSGだったらCommentReceivedが発生するか
            var data = TestHelper.GetSampleData("Streams.txt");
            var result = Tools.Parse("@badges=subscriber/12,partner/1;color=#FF0000;display-name=harutomaru;emotes=189031:20-31,60-71/588715:33-58/635709:73-82;id=9029a587-81b0-4705-8607-38cba9b762d6;mod=0;room-id=39587048;subscriber=1;tmi-sent-ts=1518062412116;turbo=0;user-id=72777405;user-type= :harutomaru!harutomaru@harutomaru.tmi.twitch.tv PRIVMSG #mimicchi :@bwscar221 おざまぁぁぁす！ mimicchiHage haruto1Harutomarubakayarou mimicchiHage bwscarDead");
            var userid = "72777405";
            var serverMock = new Mock<IDataServer>();
            serverMock.Setup(s => s.GetAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).Returns(Task.FromResult(data));
            var loggerMock = new Mock<ILogger>();
            var optionsMock = new Mock<ICommentOptions>();
            var siteOptions = new TwitchSiteOptions
            {
                NeedAutoSubNickname = true
            };
            var userStoreMock = new Mock<IUserStoreManager>();
            var userMock = new Mock<IUser>();
            userMock.SetupGet(u => u.UserId).Returns(userid);
            userStoreMock.Setup(s => s.GetUser(SiteType.Twitch, userid)).Returns(userMock.Object);
            var browserProfileMock = new Mock<IBrowserProfile>();
            var messageProvider = new MessageProvider();
            var commentDataMock = new Mock<ICommentData>();
            var commentProvider = new C(serverMock.Object, loggerMock.Object, optionsMock.Object, siteOptions, userStoreMock.Object)
            {
                MessageProvider = messageProvider,
                MetadataProvider = new MetadataProvider(),
                CommentData = commentDataMock.Object,
            };
            IMessageContext actual = null;
            commentProvider.MessageReceived += (s, e) =>
            {
                actual = e;
                commentProvider.Disconnect();
            };
            var t = commentProvider.ConnectAsync("", browserProfileMock.Object);
            messageProvider.SetResult(result);
            await t;
            var comment = actual.Message as ITwitchComment;
            Assert.AreEqual(TwitchMessageType.Comment, comment.TwitchMessageType);
            Assert.AreEqual("9029a587-81b0-4705-8607-38cba9b762d6", comment.Id);
            return;
        }
        [Test]
        public async Task 自動コテハン登録が機能するか()
        {
            var data = TestHelper.GetSampleData("Streams.txt");
            var result = Tools.Parse("@badges=subscriber/12,partner/1;color=#FF0000;display-name=harutomaru;id=9029a587-81b0-4705-8607-38cba9b762d6;mod=0;room-id=39587048;subscriber=1;tmi-sent-ts=1518062412116;turbo=0;user-id=72777405;user-type= :harutomaru!harutomaru@harutomaru.tmi.twitch.tv PRIVMSG #mimicchi :あいう @コテハン えお");
            var userid = "72777405";
            var serverMock = new Mock<IDataServer>();
            serverMock.Setup(s => s.GetAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).Returns(Task.FromResult(data));
            var loggerMock = new Mock<ILogger>();
            var optionsMock = new Mock<ICommentOptions>();
            var siteOptions = new TwitchSiteOptions
            {
                NeedAutoSubNickname = true
            };
            var userStoreMock = new Mock<IUserStoreManager>();
            var user = new UserTest(userid);
            userStoreMock.Setup(s => s.GetUser(SiteType.Twitch, userid)).Returns(user);
            var userStoreManager = userStoreMock.Object;
            var browserProfileMock = new Mock<IBrowserProfile>();
            var messageProvider = new MessageProvider();
            var commentDataMock = new Mock<ICommentData>();
            var commentProvider = new C(serverMock.Object, loggerMock.Object, optionsMock.Object, siteOptions, userStoreManager)
            {
                MessageProvider = messageProvider,
                MetadataProvider = new MetadataProvider(),
                CommentData = commentDataMock.Object,
            };
            IMessageContext actual = null;
            commentProvider.MessageReceived += (s, e) =>
            {
                actual = e;
                commentProvider.Disconnect();
            };
            var t = commentProvider.ConnectAsync("", browserProfileMock.Object);
            messageProvider.SetResult(result);
            await t;
            Assert.AreEqual("コテハン", user.Nickname);
            return;
        }
    }
}
