using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NicoSitePlugin;
using NUnit.Framework;

namespace NicoSitePluginTests
{
    [TestFixture]
    class ApiTests
    {
        [Test]
        public async Task GetCurrentCommunityLiveId()
        {
            var data = TestHelper.GetSampleData("CommunityTopHtml_onair.txt");
            var serverMock = new Mock<IDataSource>();
            serverMock.Setup(k => k.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(data));
            Assert.AreEqual("lv316750923", await API.GetCurrentCommunityLiveId(serverMock.Object, "co123", new System.Net.CookieContainer()));
        }
        [Test]
        public async Task GetCurrentChannelLiveId()
        {
            var data = TestHelper.GetSampleData("ChannelTopHtml_onair.txt");
            var serverMock = new Mock<IDataSource>();
            serverMock.Setup(k => k.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(data));
            Assert.AreEqual("lv316618092", await API.GetCurrentChannelLiveId(serverMock.Object, "ch123"));
        }
        [Test]
        public async Task GetNicoCasUserInfoTest()
        {
            var userId = "16633882";
            var data = "{\"meta\":{\"status\":200},\"data\":{\"id\":\"user/38156225\",\"contentGroups\":[{\"groupId\":\"live\",\"totalCount\":2,\"items\":[{\"type\":\"program\",\"id\":\"lv316248357\",\"description\":\"コメント表示されてない場合気付くの遅れますートーク力のない鈴音さんのおはよう配信\",\"title\":\"おはすずねー(*´꒳`*)\",\"thumbnailUrl\":\"https://nicolive.cdn.nimg.jp/tsthumb/thumbnail/181013/07/55/pg15365179441791_640_360.jpg\",\"onAirTime\":{\"beginAt\":\"2018-10-13T07:55:55+09:00\",\"endAt\":\"2018-10-13T08:41:17+09:00\"},\"showTime\":{\"beginAt\":\"2018-10-13T07:55:57+09:00\",\"endAt\":\"2018-10-13T08:41:17+09:00\"},\"viewers\":54,\"comments\":110,\"programType\":\"cas\",\"contentOwner\":{\"id\":\"38156225\",\"type\":\"user\"},\"hasArchive\":true,\"deviceFilter\":{\"isArchivePlayable\":true,\"isPlayable\":true,\"isListing\":true}},{\"type\":\"program\",\"id\":\"lv316097792\",\"description\":\"配信してるけど音声入ってるかわかりません！！！\",\"title\":\"テスト配信です⸜(* ॑ ॑* )⸝\",\"thumbnailUrl\":\"https://nicolive.cdn.nimg.jp/tsthumb/thumbnail/181006/09/48/pg14965747483263_640_360.jpg\",\"onAirTime\":{\"beginAt\":\"2018-10-06T09:48:02+09:00\",\"endAt\":\"2018-10-06T11:17:31+09:00\"},\"showTime\":{\"beginAt\":\"2018-10-06T09:48:07+09:00\",\"endAt\":\"2018-10-06T11:17:31+09:00\"},\"viewers\":110,\"comments\":142,\"programType\":\"cas\",\"contentOwner\":{\"id\":\"38156225\",\"type\":\"user\"},\"hasArchive\":true,\"deviceFilter\":{\"isArchivePlayable\":true,\"isPlayable\":true,\"isListing\":true}}],\"continuousPlayable\":true,\"options\":[{\"name\":\"sort\",\"label\":\"ソート順\",\"items\":[{\"label\":\"放送日時が近い順\",\"value\":\"liveRecent_asc\"},{\"label\":\"放送日時が遠い順\",\"value\":\"onAirTime_asc\"},{\"label\":\"来場者数が多い順\",\"value\":\"viewers_desc\"},{\"label\":\"コメント数が多い順\",\"value\":\"comments_desc\"}]}],\"defaultOption\":[{\"name\":\"sort\",\"value\":\"liveRecent_asc\"}],\"label\":\"このユーザーの番組\",\"annotation\":{\"preferredDisplayContents\":10}}],\"messageServer\":{\"wss\":\"wss://nmsg.nicovideo.jp:2544/websocket\",\"ws\":\"ws://nmsg.nicovideo.jp:2581/websocket\",\"https\":\"https://nmsg.nicovideo.jp:2543/api\",\"version\":20061206,\"id\":\"TZ38156225\",\"service\":\"HIROBA\"},\"name\":\"鈴音さん@ばーちゃる\",\"owner\":{\"id\":\"38156225\",\"name\":\"鈴音さん@ばーちゃる\",\"icon\":\"https://secure-dcdn.cdn.nimg.jp/nicoaccount/usericon/3815/38156225.jpg?1538744712\"},\"updateAt\":\"2018-10-13T15:18:54+0900\",\"follow\":{\"type\":\"owner\"},\"notification\":{\"type\":\"owner\"},\"annotation\":{\"player\":{\"expandDetail\":true}},\"icon\":\"https://secure-dcdn.cdn.nimg.jp/nicoaccount/usericon/3815/38156225.jpg?1538744712\",\"priority\":0}}";
            var serverMock = new Mock<IDataSource>();
            serverMock.Setup(k => k.GetAsync("https://api.cas.nicovideo.jp/v1/tanzakus/user/" + userId)).Returns(Task.FromResult(data));
            var userInfo = await API.GetNicoCasUserInfo(serverMock.Object, userId);
            Assert.AreEqual("鈴音さん@ばーちゃる", userInfo.Name);
            Assert.AreEqual("wss://nmsg.nicovideo.jp:2544/websocket", userInfo.MessageServerUrlWss);
            Assert.AreEqual("おはすずねー(*´꒳`*)", userInfo.Lives[0].Title);
            Assert.AreEqual("テスト配信です⸜(* ॑ ॑* )⸝", userInfo.Lives[1].Title);


        }
        [Test]
        public async Task GetJikkyoInfoTest()
        {
            var serverMock = new Mock<IDataSource>();
            var channelId = 4;
            var s = "done=true&thread_id=1531854003&ms=202.219.109.199&ms_port=2526&http_port=8081&channel_no=4&channel_name=%E6%97%A5%E6%9C%AC%E3%83%86%E3%83%AC%E3%83%93&genre_id=1&twitter_enabled=1&vip_follower_disabled=0&twitter_vip_mode_count=10000&twitter_hashtag=%23NTV&twitter_api_url=http%3A%2F%2Fjk.nicovideo.jp%2Fapi%2Fv2%2F&base_time=1531854001&open_time=1531854002&start_time=1531854003&end_time=1531877719&user_id=2297426&is_premium=0&nickname=Ryu";
            serverMock.Setup(k => k.GetAsync("http://jk.nicovideo.jp/api/v2/getflv?v=jk" + channelId)).Returns(Task.FromResult(s));
            var m = await API.GetJikkyoInfoAsync(serverMock.Object, channelId);
            Assert.AreEqual("日本テレビ", m.Name);
            Assert.AreEqual("202.219.109.199", m.XmlSocketAddr);
            Assert.AreEqual(2526, m.XmlSocketPort);
            Assert.AreEqual("1531854003", m.ThreadId);
            Assert.AreEqual("1531854001", m.BaseTime);
            Assert.AreEqual("1531854002", m.OpenTime);
            Assert.AreEqual("1531854003", m.StartTime);
            Assert.AreEqual("1531877719", m.EndTime);
        }
        [Test]
        public void GetJikkyoInfoFaildTest()
        {
            var serverMock = new Mock<IDataSource>();
            var channelId = 400;
            var s = "code=1&error=invalid_thread&done=true";
            serverMock.Setup(k => k.GetAsync("http://jk.nicovideo.jp/api/v2/getflv?v=jk" + channelId)).Returns(Task.FromResult(s));
            Assert.ThrowsAsync<JikkyoInfoFailedException>(async () => await API.GetJikkyoInfoAsync(serverMock.Object, channelId));
        }
        [Test]
        public async Task GetUserInfoTest()
        {
            var serverMock = new Mock<IDataSource>();
            var data = "{\"nicovideo_user_response\":{\"user\":{\"id\":\"2297426\",\"nickname\":\"Ryu\",\"thumbnail_url\":\"http:\\/\\/dcdn.cdn.nimg.jp\\/nicoaccount\\/usericon\\/229\\/2297426.jpg?1477771628\"},\"vita_option\":{\"user_secret\":\"0\"},\"additionals\":\"\",\"@status\":\"ok\"}}";
            serverMock.Setup(s => s.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(data));
            var userInfo = await API.GetUserInfo(serverMock.Object, "");
            Assert.AreEqual("2297426", userInfo.UserId);
            Assert.AreEqual("Ryu", userInfo.Name);
            Assert.AreEqual("http://dcdn.cdn.nimg.jp/nicoaccount/usericon/229/2297426.jpg?1477771628", userInfo.ThumbnailUrl);
        }
        [Test]
        public void GetUserInfoThrowTest()
        {
            var serverMock = new Mock<IDataSource>();
            var data = "{\"nicovideo_user_response\":{\"error\":{\"code\":\"NOT_FOUND\",\"description\":\"\\u30e6\\u30fc\\u30b6\\u30fc\\u304c\\u898b\\u3064\\u304b\\u308a\\u307e\\u305b\\u3093\"},\"@status\":\"fail\"}}";
            serverMock.Setup(s => s.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(data));
            Assert.ThrowsAsync<ParseException>(()=>API.GetUserInfo(serverMock.Object, ""));
        }
    }
}
