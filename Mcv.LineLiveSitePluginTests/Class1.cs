using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using LineLiveSitePlugin;
using LineLiveSitePlugin.ParseMessage;
namespace LineLiveSitePluginTests
{
    [TestFixture]
    public class Class1
    {
        const string _data = "{\"liveHLSURLs\":{\"abr\":\"https://lss.line-scdn.net/cc_jp/p/live/hZfOCtserBkglVxpYGkIZf0k-W38LC0cbTFgaJ1M6Wy1WWUIeSA5OK1M6C35dX0RLHlxCelQzU3BWT0MWTAwefFc8RSQdSB5JDUMXdwhuRCULAUse/abr.m3u8\",\"aac\":\"https://lss.line-scdn.net/cc_jp/p/live/hc0MRXpjPPx4sWiUKCQ4jIRA1MX9DTR1LTQt2KSgsa3QLfRIsEzgoATJZFixZbXtOBDgSEi41Yi5CG3hAU01xcUorYCxWG3NLSA/aac/chunklist.m3u8\",\"720\":\"https://lss.line-scdn.net/cc_jp/p/live/hc0MRXpjPPx4sWiUKCQ4jIRA1MX9DTR1LTQt2KSgsa3QLfRIsEzgoATJZFixZbXtOBDgSEi41Yi5CG3hAU01xcUorYCxWG3NLSA/720/chunklist.m3u8\",\"480\":\"https://lss.line-scdn.net/cc_jp/p/live/hc0MRXpjPPx4sWiUKCQ4jIRA1MX9DTR1LTQt2KSgsa3QLfRIsEzgoATJZFixZbXtOBDgSEi41Yi5CG3hAU01xcUorYCxWG3NLSA/480/chunklist.m3u8\",\"360\":\"https://lss.line-scdn.net/cc_jp/p/live/hc0MRXpjPPx4sWiUKCQ4jIRA1MX9DTR1LTQt2KSgsa3QLfRIsEzgoATJZFixZbXtOBDgSEi41Yi5CG3hAU01xcUorYCxWG3NLSA/360/chunklist.m3u8\",\"240\":null,\"144\":null},\"archivedHLSURLs\":{\"abr\":null,\"aac\":null,\"720\":null,\"480\":null,\"360\":null,\"240\":null,\"144\":null},\"ad\":null,\"pinnedMessage\":null,\"badges\":[],\"supportGauge\":null,\"apistatusCode\":200,\"item\":{\"id\":8376753,\"channelId\":21,\"title\":\"\\u30AC\\u30EB\\u30A2\\u30EF\\u751F\\u653E\\u9001\\u301C2018SS\\u301C\\u958B\\u6F14\\u76F4\\u524DSP!!\",\"viewerCount\":75092,\"loveCount\":166423,\"freeLoveCount\":166191,\"premiumLoveCount\":232,\"chatCount\":1441,\"thumbnailURLs\":{\"small\":\"https://scdn.line-apps.com/obs/r/live/ba/517e04535a9511e8b107c5551a63d7ea19d28988t083aeb17__lastscene.jpg/f375x210?_=25445111\",\"large\":\"https://scdn.line-apps.com/obs/r/live/ba/517e04535a9511e8b107c5551a63d7ea19d28988t083aeb17__lastscene.jpg/f960x540?_=25445111\",\"commonLarge\":\"https://scdn.line-apps.com/obs/r/live/ba/517e04535a9511e8b107c5551a63d7ea19d28988t083aeb17__lastscene.jpg/f960x720?_=25445111\",\"commonSmall\":\"https://scdn.line-apps.com/obs/r/live/ba/517e04535a9511e8b107c5551a63d7ea19d28988t083aeb17__lastscene.jpg/f375x281?_=25445111\",\"large1x1\":\"https://scdn.line-apps.com/obs/r/live/ba/517e04535a9511e8b107c5551a63d7ea19d28988t083aeb17__lastscene.jpg/f960x960?_=25445111\",\"small1x1\":\"https://scdn.line-apps.com/obs/r/live/ba/517e04535a9511e8b107c5551a63d7ea19d28988t083aeb17__lastscene.jpg/f375x375?_=25445111\",\"landscape\":\"https://scdn.line-apps.com/obs/r/live/ba/517e04535a9511e8b107c5551a63d7ea19d28988t083aeb17__lastscene.jpg/f960x540?_=25445111\",\"swipe\":\"https://scdn.line-apps.com/obs/r/live/ba/517e04535a9511e8b107c5551a63d7ea19d28988t083aeb17__lastscene.jpg/f540x960?_=25445111\"},\"autoPlayURL\":\"http://lss.line-cdn.net/p/live/hc0MRXpjPPx4sWiUKCQ4jIRA1MX9DTR1LTQt2KSgsa3QLfRIsEzgoATJZFixZbXtOBDgSEi41Yi5CG3hAU01xcUorYCxWG3NLSA/360/chunklist.m3u8\",\"vodLastsceneURL\":null,\"shareURL\":\"https://live.line.me/channels/21/broadcast/8376753\",\"broadcastSecretToken\":null,\"numericAspectRatio\":0.5625,\"liveStatus\":\"LIVE\",\"archiveStatus\":\"RECORDING\",\"archiveDuration\":0,\"finishedBroadcastingAt\":0,\"createdAt\":1526705733,\"updatedAt\":1526706004,\"channel\":{\"id\":21,\"name\":\"LIVE \\u30C1\\u30E3\\u30F3\\u30CD\\u30EB\",\"iconURL\":\"https://scdn.line-apps.com/obs/0h1jyawc06bkVEGEHSdXEREn1FaDI9NntLZix4cz5HZCh1e3dWZH4iKmcfNXV1KCFGeXwpK2FdMXNqei4Reyo/f318x318\",\"statusMessage\":\"\\u6709\\u540D\\u4EBA\\u306E\\u751F\\u914D\\u4FE1\\u304C\\u898B\\u3089\\u308C\\u308B\\u266A\",\"type\":\"OFFICIAL_ACCOUNT\",\"isFollowing\":null,\"mid\":\"u64984ee83b048086e587d7eb464bedcc\"},\"tags\":[\"\\u30AC\\u30EB\\u30A2\\u30EF\"],\"apistatusCode\":200,\"isBroadcastingNow\":true,\"isOAFollowRequired\":false,\"isArchived\":false,\"isBanned\":false,\"isRadioMode\":false,\"isCollaboratable\":false},\"chat\":{\"url\":\"wss://cast-chat-026.line-apps.com/chat/app/8376753/WzKgAtbWPPJbtbXwPMxvMCeUibBqXyqg\",\"archiveURL\":null,\"ownerMessageURL\":\"https://cast-chat-api.line-apps.com/channel/8376753/owner_message/list\"},\"isFollowing\":null,\"isOAFollowRequired\":false,\"isChannelBlocked\":false,\"lsaPath\":\"live/ba/517e04535a9511e8b107c5551a63d7ea19d28988t083aeb17\",\"archiveBroadcastEndAt\":null,\"currentServerEpoch\":1526706676,\"user\":{\"twitterScreenName\":null,\"facebookUserName\":null,\"hasLineAccount\":false},\"description\":\"5\\u670819\\u65E5(\\u571F)\\u306B\\u958B\\u50AC\\u3055\\u308C\\u308B\\u300ERakuten GirlsAward 2018 SPRING/SUMMER\\u300F\\u306E\\u69D8\\u5B50\\u3092\\u3001\\u4F1A\\u5834\\u306E\\u5E55\\u5F35\\u30E1\\u30C3\\u30BB\\u304B\\u3089\\u3001\\u7D046\\u6642\\u9593\\u306B\\u308F\\u305F\\u308A\\u751F\\u4E2D\\u7D99\\uFF01\\n\\n\\u767D\\u77F3\\u9EBB\\u8863\\u3001\\u85E4\\u7530\\u30CB\\u30B3\\u30EB\\u3001\\u6EDD\\u6CA2\\u30AB\\u30EC\\u30F3\\u3001\\u7389\\u57CE\\u30C6\\u30A3\\u30CA\\u3001emma\\u306A\\u3069\\u7DCF\\u52E2150\\u540D\\u4EE5\\u4E0A\\u306E\\u8D85\\u8C6A\\u83EF\\u51FA\\u6F14\\u8005\\u306B\\u52A0\\u3048\\u30FB\\u30FB\\u30FB\\u307E\\u3060\\u767A\\u8868\\u3055\\u308C\\u3066\\u3044\\u306A\\u3044\\u30B7\\u30FC\\u30AF\\u30EC\\u30C3\\u30C8\\u30B2\\u30B9\\u30C8\\u3082\\u767B\\u5834\\u2661\\n\\n\\u307E\\u305F\\u3001\\u4F1A\\u5834\\u5185\\u306E\\u7279\\u8A2D\\u30B9\\u30BF\\u30B8\\u30AA\\u3067\\u306F\\u3001\\u51FA\\u6F14\\u8005\\u3078\\u306E\\u30B9\\u30DA\\u30B7\\u30E3\\u30EB\\u30A4\\u30F3\\u30BF\\u30D3\\u30E5\\u30FC\\u3084\\u30D7\\u30EC\\u30BC\\u30F3\\u30C8\\u4F01\\u753B\\u306A\\u3069\\u3001\\u76DB\\u308A\\u3060\\u304F\\u3055\\u3093\\u3067\\u304A\\u9001\\u308A\\u3044\\u305F\\u3057\\u307E\\u3059\\uFF01\\n\\n\\u6700\\u521D\\u304B\\u3089\\u6700\\u5F8C\\u307E\\u3067\\u304A\\u898B\\u9003\\u3057\\u306A\\u304F\\u2661\\u305C\\u3072\\u3001\\u3054\\u89A7\\u304F\\u3060\\u3055\\u3044\\u266A\",\"isUseGift\":true,\"category\":\"Entertainment\",\"hotCasts\":[],\"hotCastsWithSection\":{\"broadcastingBroadcasts\":[],\"categoryPopularBroadcasts\":[],\"homeHotBroadcasts\":[]},\"isRadioMode\":false,\"currentViewerCount\":null,\"isCollaborating\":false,\"isCollaboratable\":false,\"canRequestCollaboration\":true,\"isTrivia\":false,\"status\":200}";
        [Test]
        public void Test()
        {
            var s = Tools.Deserialize<LineLiveSitePlugin.Low.LiveInfo.RootObject>(_data);
            Assert.AreEqual("wss://cast-chat-026.line-apps.com/chat/app/8376753/WzKgAtbWPPJbtbXwPMxvMCeUibBqXyqg", s.Chat.Url);
        }

        [Test]
        public void Test2()
        {
            //https://live.line.me/channels/191159/broadcast/8383310

        }
        [Test]
        public void MessageParseTest()
        {
            var s = "{\"type\":\"message\",\"data\":{\"message\":\"え、この歌もええ！！！\",\"sender\":{\"id\":7781129,\"hashedId\":\"YaBPX9N1BF\",\"displayName\":\"abc\",\"iconUrl\":\"https://scdn.line-apps.com/obs/0h786ri4KAaBcLC0S_mDIXQDNWbmByJWtfcy9zMjINY3QvaXoTZ20uJXsLZHRxP3pDNzpwJS5YNnMhOS1IMA/f64x64\",\"hashedIconId\":\"0h786ri4KAaBcLC0S_mDIXQDNWbmByJWtfcy9zMjINY3QvaXoTZ20uJXsLZHRxP3pDNzpwJS5YNnMhOS1IMA\",\"isGuest\":false,\"isBlocked\":false},\"sentAt\":1526748828,\"isNGMessage\":false,\"key\":\"3127985.7781129000000000\",\"roomId\":\"gypkYEkazJsEmdZnqlTmFTrraGbyJnPG\"}}";
            var (data, sender) = Tools.Parse(s);
            var message = data as IMessageData;
            Assert.IsNotNull(message);
            Assert.AreEqual("え、この歌もええ！！！", message.Message);
            Assert.AreEqual("abc", sender.DisplayName);
        }
        [Test]
        public void LoveParseTest()
        {
            var s = "{\"type\":\"love\",\"data\":{\"quantity\":1,\"sender\":{\"id\":4555519,\"hashedId\":\"6cuz94hm5I\",\"displayName\":\"よっちゃん\",\"iconUrl\":\"https://scdn.line-apps.com/obs/0hINkqtsQ5FmFWCDkVBrNpNm5VEBYvJhUpLiwNRG8MGgEpOVdibWdeBHEBS1J7bFU0P2tbBnALQFUuPFhgOA/f64x64\",\"hashedIconId\":\"0hINkqtsQ5FmFWCDkVBrNpNm5VEBYvJhUpLiwNRG8MGgEpOVdibWdeBHEBS1J7bFU0P2tbBnALQFUuPFhgOA\",\"isGuest\":false,\"isBlocked\":false},\"roomId\":\"gypkYEkazJsEmdZnqlTmFTrraGbyJnPG\",\"sentAt\":1526748863}}";
            var (data, sender) = Tools.Parse(s);
            var message = data as ILove;
            Assert.AreEqual(1, message.Quantity);
        }
        [Test]
        public void BulkParseTest()
        {
            var s = TestHelper.GetSampleData("sample_bulk.txt");
            var message = Tools.Parse(s);
            return;
        }
    }
}
