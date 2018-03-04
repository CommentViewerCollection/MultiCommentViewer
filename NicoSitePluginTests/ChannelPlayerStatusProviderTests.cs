using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NicoSitePlugin;
using NicoSitePlugin.Old;
using NicoSitePlugin.Test2;
using Moq;
using System.Net;

namespace NicoSitePluginTests
{
    [TestFixture]
    class ChannelPlayerStatusProviderTests
    {
        const string ps = "<?xml version=\"1.0\" encoding=\"utf-8\"?><getplayerstatus status=\"ok\" time=\"1520155462\"><stream><id>lv311475814</id><title>テスト</title><description>ああ</description><provider_type>community</provider_type><default_community>co1034396</default_community><international>13</international><is_owner>1</is_owner><owner_id>2297426</owner_id><owner_name>Ryu</owner_name><is_reserved>0</is_reserved><is_niconico_enquete_enabled>1</is_niconico_enquete_enabled><watch_count>0</watch_count><comment_count>0</comment_count><base_time>1520155441</base_time><open_time>1520155441</open_time><start_time>1520157241</start_time><end_time>1520159041</end_time><is_rerun_stream>0</is_rerun_stream><bourbon_url>http://live.nicovideo.jp/gate/lv311475814?sec=nicolive_crowded&amp;sub=watch_crowded_0_community_lv311475814_onair</bourbon_url><full_video>http://live.nicovideo.jp/gate/lv311475814?sec=nicolive_crowded&amp;sub=watch_crowded_0_community_lv311475814_onair</full_video><after_video></after_video><before_video></before_video><kickout_video>http://live.nicovideo.jp/gate/lv311475814?sec=nicolive_oidashi&amp;sub=watchplayer_oidashialert_0_community_lv311475814_onair</kickout_video><twitter_tag>#co1034396</twitter_tag><danjo_comment_mode>0</danjo_comment_mode><infinity_mode>0</infinity_mode><archive>0</archive><press><display_lines>-1</display_lines><display_time>-1</display_time><style_conf></style_conf></press><plugin_delay></plugin_delay><plugin_url></plugin_url><plugin_urls/><allow_netduetto>0</allow_netduetto><broadcast_token>a0855e3b31e2d783d19e8e49eaed044526292e4c</broadcast_token><ng_scoring>0</ng_scoring><is_nonarchive_timeshift_enabled>0</is_nonarchive_timeshift_enabled><is_timeshift_reserved>0</is_timeshift_reserved><header_comment>0</header_comment><footer_comment>0</footer_comment><split_bottom>0</split_bottom><split_top>0</split_top><background_comment>0</background_comment><font_scale></font_scale><comment_lock>0</comment_lock><telop><enable>0</enable></telop><contents_list><contents id=\"main\" disableAudio=\"0\" disableVideo=\"0\" start_time=\"1520155461\">rtmp:rtmp://nlpoca145.live.nicovideo.jp:1935/publicorigin/180304_18_0/,lv311475814?1520155462:30:8d090cca570c4a34</contents></contents_list><picture_url>https://secure-dcdn.cdn.nimg.jp/comch/community-icon/128x128/co1034396.jpg?1489263513</picture_url><thumb_url>https://secure-dcdn.cdn.nimg.jp/comch/community-icon/64x64/co1034396.jpg?1489263513</thumb_url><is_priority_prefecture></is_priority_prefecture></stream><user><user_id>2297426</user_id><nickname>Ryu</nickname><is_premium>1</is_premium><userAge>29</userAge><userSex>1</userSex><userDomain>jp</userDomain><userPrefecture>14</userPrefecture><userLanguage>ja-jp</userLanguage><room_label>co1034396</room_label><room_seetno>2525502</room_seetno><is_join>1</is_join><twitter_info><status>enabled</status><screen_name>namazu2525</screen_name><followers_count>8990</followers_count><is_vip>0</is_vip><profile_image_url>http://pbs.twimg.com/profile_images/2150344550/20120420070739576_normal.jpg</profile_image_url><after_auth>0</after_auth><tweet_token>eacbae97a1c148356f6236a39992072064d94934</tweet_token></twitter_info></user><rtmp is_fms=\"1\" rtmpt_port=\"80\"><url>rtmp://nleu22.live.nicovideo.jp:1935/liveedge/live_180304_18_10</url><ticket>2297426:lv311475814:4:1520157241:0c26ebba34ba8b06</ticket></rtmp><ms><addr>msg105.live.nicovideo.jp</addr><port>2847</port><thread>1622435441</thread></ms><tid_list><tid>1622435441</tid><tid>1622435442</tid></tid_list><ms_list><ms><addr>msg105.live.nicovideo.jp</addr><port>2847</port><thread>1622435441</thread></ms><ms><addr>msg101.live.nicovideo.jp</addr><port>2808</port><thread>1622435442</thread></ms></ms_list><twitter><live_enabled>1</live_enabled><vip_mode_count>10000</vip_mode_count><live_api_url>http://watch.live.nicovideo.jp/api/</live_api_url></twitter><player><qos_analytics>0</qos_analytics><dialog_image><oidashi>http://nicolive.cdn.nimg.jp/live/simg/img/201311/281696.a29344.png</oidashi></dialog_image><is_notice_viewer_balloon_enabled>1</is_notice_viewer_balloon_enabled><error_report>1</error_report></player><marquee><category>一般(その他)</category><game_key>823dd39b</game_key><game_time>1520155462</game_time><force_nicowari_off>1</force_nicowari_off></marquee></getplayerstatus>";
        [Test]
        public async Task Test()
        {
            var liveId = "lv311475814";
            var ccMock = new Mock<CookieContainer>();
            var serverMock = new Mock<IDataSource>();
            serverMock.Setup(m => m.Get("http://live.nicovideo.jp/api/getplayerstatus?v="+liveId, ccMock.Object)).ReturnsAsync(ps);
            serverMock.Setup(m => m.Get("http://int-main.net/api/playerstatus/" + liveId, ccMock.Object)).ReturnsAsync(ps);
            var psProvider = new ChannelCommunityPlayerStatusProvider(serverMock.Object, liveId, 60 * 1000, ccMock.Object);
            bool b = false;
            int n = 0;
            psProvider.Received += (s, e) =>
            {
                if(n == 0)
                {
                    Assert.AreEqual("テスト", e.Title);
                }
                else if(n == 1)
                {
                    Assert.AreEqual("テスト", e.Title);
                    b = true;
                    psProvider.Disconnect();
                }
                n++;
            };
            await psProvider.ReceiveAsync();
            Assert.IsTrue(b);
        }
    }
}
