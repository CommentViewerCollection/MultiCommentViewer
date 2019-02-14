using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Common;
using LineLiveSitePlugin;
using Moq;
using NUnit.Framework;
using ryu_s.BrowserCookie;
using SitePlugin;

namespace LineLiveSitePluginTests
{
    class LineLiveCommentProvider_LiveIdを含んだURLが入力された場合Tests
    {
        class Mp : IMessageProvider
        {
            public event EventHandler Opened;
            public event EventHandler<string> Received;

            public void Disconnect()
            {
                throw new NotImplementedException();
            }

            public Task ReceiveAsync(string url)
            {
                return Task.CompletedTask;
            }

            public Task SendAsync(string s)
            {
                throw new NotImplementedException();
            }
        }
        //チャンネルURLが入力された場合は放送が終了しても一定時間毎にチャンネルを監視して、放送が始まったら自動的に接続する。
        class C : LineLiveCommentProvider
        {
            protected override IMessageProvider CreateMessageProvider()
            {
                return new Mp();
            }
            protected override CookieContainer GetCookieContainer(IBrowserProfile browserProfile)
            {
                return new CookieContainer();
            }
            protected override (string channelId, string liveId) GetLiveIdFromInput(string input)
            {
                return ("channelId", "liveId");
            }
            public ILiveInfo LiveInfo { get; set; }
            public event EventHandler BeforeGetLiveInfo;
            protected override Task<(ILiveInfo, string raw)> GetLiveInfo(string channelId, string liveId)
            {
                BeforeGetLiveInfo?.Invoke(this, EventArgs.Empty);
                return Task.FromResult((LiveInfo, ""));
            }
            protected override Task InitLoveIconUrlDict()
            {
                _loveIconUrlDict = new Dictionary<string, string>();
                return Task.CompletedTask;
            }
            public C(IDataServer server, ILogger logger, ICommentOptions options, LineLiveSiteOptions siteOptions, IUserStore userStore)
                :base(server, logger, options, siteOptions, userStore)
            {

            }
            protected override Task AfterMessageProviderDisconnected()
            {

                return Task.CompletedTask;
            }
        }
        class Server : IDataServer
        {
            public Task<string> GetAsync(string url)
            {
                throw new NotImplementedException();
            }

            public Task<string> GetAsync(string url, Dictionary<string, string> headers, CookieContainer cc)
            {
                throw new NotImplementedException();
            }
        }
        class Logger : ILogger
        {
            public string GetExceptions() => "";

            public void LogException(Exception ex, string message = "", string detail = "") { }
        }
        [Test]
        public async Task LiveIdを指定されたが既に終了していた場合即座に切断する()
        {
            //放送URLが入力された場合はその放送が終了し次第切断する。
            await Test(new List<string> { "FINISHED" }, 1);
        }
        [Test]
        public async Task 放送URLが入力された場合はその放送が終了し次第切断する()
        {
            var list = new List<string>
            {
                "LIVE",
                "LIVE",
                "FINISHED",
            };
            await Test(list, 3);
        }
        private async Task Test(List<string> list, int expected)
        {
            var server = new Server();
            var logger = new Logger();
            var optionsMock = new Mock<ICommentOptions>();
            var siteOptions = new LineLiveSiteOptions();
            var userStoreMock = new Mock<IUserStore>();
            var provider = new C(server, logger, optionsMock.Object, siteOptions, userStoreMock.Object);
            int n = 0;
            provider.BeforeGetLiveInfo += (s, e) =>
            {
                provider.LiveInfo = new LiveInfo
                {
                    LiveStatus = list[n++],
                };
            };
            var providerTask = provider.ConnectAsync("", null);
            var k = Task.Delay(5000);
            var t = await Task.WhenAny(providerTask, k);
            Assert.IsTrue(t == providerTask);
            Assert.AreEqual(expected, n);
        }
    }
    class LineLiveCommentProvider_ChannelIdのみを含んだURLが入力された場合2Tests
    {
        class Mp : IMessageProvider
        {
            public event EventHandler Opened;
            public event EventHandler<string> Received;

            public void Disconnect()
            {
                throw new NotImplementedException();
            }

            public Task ReceiveAsync(string url)
            {
                return Task.CompletedTask;
            }

            public Task SendAsync(string s)
            {
                throw new NotImplementedException();
            }
        }
        
        class C : LineLiveCommentProvider
        {
            protected override IMessageProvider CreateMessageProvider()
            {
                return new Mp();
            }
            protected override CookieContainer GetCookieContainer(IBrowserProfile browserProfile)
            {
                return new CookieContainer();
            }
            protected override (string channelId, string liveId) GetLiveIdFromInput(string input)
            {
                return ("channelId", null);
            }
            public ILiveInfo LiveInfo { get; set; }
            public event EventHandler BeforeGetLiveInfo;
            protected override Task<(ILiveInfo, string raw)> GetLiveInfo(string channelId, string liveId)
            {
                BeforeGetLiveInfo?.Invoke(this, EventArgs.Empty);
                return Task.FromResult((LiveInfo, ""));
            }
            public string LiveId { get; set; }
            public event EventHandler BeforeGetLiveId;
            protected override Task<string> GetLiveIdFromChannelId(IDataServer server, string channelId)
            {
                BeforeGetLiveId?.Invoke(this, EventArgs.Empty);
                if (string.IsNullOrEmpty(LiveId)) throw new LiveNotFoundException();
                return Task.FromResult(LiveId);
            }
            protected override Task InitLoveIconUrlDict()
            {
                _loveIconUrlDict = new Dictionary<string, string>();
                return Task.CompletedTask;
            }
            public C(IDataServer server, ILogger logger, ICommentOptions options, LineLiveSiteOptions siteOptions, IUserStore userStore)
                : base(server, logger, options, siteOptions, userStore)
            {

            }
            int i = 0;
            public int Expired { get; set; }
            protected override Task AfterMessageProviderDisconnected()
            {
                if (i == Expired) throw new Exception();
                return Task.CompletedTask;
            }
        }
        class Server : IDataServer
        {
            public Task<string> GetAsync(string url)
            {
                throw new NotImplementedException();
            }

            public Task<string> GetAsync(string url, Dictionary<string, string> headers, CookieContainer cc)
            {
                throw new NotImplementedException();
            }
        }
        class Logger : ILogger
        {
            public string GetExceptions() => "";

            public void LogException(Exception ex, string message = "", string detail = "") { }
        }
        [Test]
        public async Task チャンネルURLが入力された場合は放送が終了しても一定時間毎にチャンネルを監視して放送が始まったら自動的に接続する()
        {
            var listk = new List<(string, ILiveInfo)>
            {
                ("", new LiveInfo{ LiveStatus ="FINISHED"}),
                ("", new LiveInfo{ LiveStatus ="FINISHED"}),
                ("abc", new LiveInfo{ LiveStatus ="LIVE"}),
                ("akb", new LiveInfo{ LiveStatus ="LIVE"}),
                ("", new LiveInfo{ LiveStatus ="FINISHED"}),
                ("", new LiveInfo{ LiveStatus ="FINISHED"}),

            };
            await Test(listk, 6);
        }
        [Test]
        public async Task 配信が始まるまでひたすら待つ()
        {
            //放送URLが入力された場合はその放送が終了し次第切断する。
            var n = 10;
            var list = Enumerable.Repeat("", n).ToList();
            var tupple = ("", ((ILiveInfo)new LiveInfo { LiveStatus = "FINISHED" }));
            var list2 = Enumerable.Repeat(tupple, n).ToList();
            await Test(list2, list.Count);
        }
        private async Task Test(List<(string liveId,ILiveInfo liveInfo)> list2, int expected)
        {
            var server = new Server();
            var logger = new Logger();
            var optionsMock = new Mock<ICommentOptions>();
            var siteOptions = new LineLiveSiteOptions();
            var userStoreMock = new Mock<IUserStore>();
            var provider = new C(server, logger, optionsMock.Object, siteOptions, userStoreMock.Object);
            provider.LiveCheckIntervalMs = 0;
            int n = 0;
            provider.BeforeGetLiveId += (s, e) =>
            {
                provider.LiveId = list2[n++].liveId;
            };
            provider.BeforeGetLiveInfo += (s, e) =>
            {
                provider.LiveInfo = list2[n].liveInfo;
            };
            provider.Expired = expected;
            
            var providerTask = provider.ConnectAsync("", null);
            var k = Task.Delay(5000);
            var t = await Task.WhenAny(providerTask, k);
            Assert.IsTrue(t == providerTask);
            Assert.AreEqual(expected+1, n);
        }
    }
}
