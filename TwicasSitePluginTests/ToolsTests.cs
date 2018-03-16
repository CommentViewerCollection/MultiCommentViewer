using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwicasSitePlugin;
using TwicasSitePlugin.LowObject;
using Newtonsoft.Json;
using NUnit.Framework;
using SitePlugin;
using Common;

namespace TwicasSitePluginTests
{
    [TestFixture]
    public class ToolsTests
    {
        [Test]
        public void Twicas_ParseCommentTest()
        {
            var s = "[{\"id\":12623240379,\"class\":\"other\",\"html\":\"<td class=\\\"img\\\"><a href=\\\"#\\\" onclick=\\\"javascript:show_follow_option('nd7m0ix');return false;\\\"><img src=\\\"\\/\\/imagegw02.twitcasting.tv\\/image3s\\/abs.twimg.com\\/sticky\\/default_profile_images\\/default_profile_normal.png\\\" width=\\\"48\\\" height=\\\"48\\\"><\\/a><\\/td><td class=\\\"comment\\\"><span class=\\\"user\\\"><a href=\\\"#\\\" onclick=\\\"javascript:show_follow_option('nd7m0ix');return false;\\\">\\u2606<\\/a><span class=\\\"name\\\">@nd7m0ix<\\/span><\\/span><br><span class=\\\"comment-text\\\">\\u3060\\u3063\\u305f\\u3089\\u30b9\\u30ed\\u914d\\u4fe1\\u3059\\u3093\\u306aw<\\/span><\\/td><td><a href=\\\"\\/usam0m0nana\\/comment\\/441581069-12623240379\\\"  target=\\\"_blank\\\"><img src=\\\"\\/\\/imagegw03.twitcasting.tv\\/image3\\/image.twitcasting.tv\\/image73_1\\/0d\\/fe\\/1a51fe0d-83084-s.jpg\\\" class=\\\"commentthumb\\\"><\\/a><\\/td>\",\"date\":\"Thu, 15 Feb 2018 15:31:50 +0900\",\"dur\":\"47:34\",\"uid\":\"nd7m0ix\",\"screen\":\"nd7m0ix\",\"statusid\":\"\",\"lat\":0,\"lng\":0,\"show\":true,\"yomi\":\"\"},{\"id\":12623240486,\"class\":\"other\",\"html\":\"<td class=\\\"img\\\"><a href=\\\"#\\\" onclick=\\\"javascript:show_follow_option('satoyan0810');return false;\\\"><img src=\\\"\\/\\/imagegw02.twitcasting.tv\\/image3s\\/pbs.twimg.com\\/profile_images\\/634972230590447616\\/hIr5kBT9_normal.jpg\\\" width=\\\"48\\\" height=\\\"48\\\"><\\/a><\\/td><td class=\\\"comment\\\"><img class=\\\"premium\\\" src=\\\"\\/img\\/icon_premium_star1.png\\\"><span class=\\\"user\\\"><a href=\\\"#\\\" onclick=\\\"javascript:show_follow_option('satoyan0810');return false;\\\">\\u4ecf\\u306e\\u30b5\\u30c8\\u3084\\u3093\\u4ecf\\u306e\\u5fc3\\u306e\\u6301\\u3061\\u4e3b<\\/a><span class=\\\"name\\\">@satoyan0810<\\/span><\\/span><br><span class=\\\"comment-text\\\">\\u30ad\\u30e5\\u30d4\\u30fc\\u30f3<\\/span><\\/td><td><a href=\\\"\\/usam0m0nana\\/comment\\/441581069-12623240486\\\"  target=\\\"_blank\\\"><img src=\\\"\\/\\/imagegw03.twitcasting.tv\\/image3\\/image.twitcasting.tv\\/image73_1\\/0d\\/fe\\/1a51fe0d-83185-s.jpg\\\" class=\\\"commentthumb\\\"><\\/a><\\/td>\",\"date\":\"Thu, 15 Feb 2018 15:31:52 +0900\",\"dur\":\"47:36\",\"uid\":\"satoyan0810\",\"screen\":\"satoyan0810\",\"statusid\":\"\",\"lat\":0,\"lng\":0,\"show\":true,\"yomi\":\"\"}]";
            var json = JsonConvert.DeserializeObject<Comment[]>(s);
            var low = json[0];
            var data = Tools.Parse(low);
            Assert.AreEqual(12623240379, data.Id);
            Assert.AreEqual("nd7m0ix", data.UserId);
            Assert.AreEqual("☆", data.Name);
            CollectionAssert.AreEquivalent(new List<IMessagePart> { new MessageText("だったらスロ配信すんなw") }, data.Message);
            Assert.AreEqual("http://imagegw02.twitcasting.tv/image3s/abs.twimg.com/sticky/default_profile_images/default_profile_normal.png", data.ThumbnailUrl);
        }
        [Test]
        public void Twicas_IsValidUrl()
        {
            Assert.IsTrue(Tools.IsValidUrl("https://twitcasting.tv/chara2shinai"));
        }
    }
}
