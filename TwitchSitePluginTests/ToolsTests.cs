using System;
using TwitchSitePlugin;
using System.Net;
using NUnit.Framework;

namespace TwitchSitePluginTests
{
    [TestFixture]
    public class ToolsTests
    {
        [Test]
        public void ResultTest()
        {
            var t = Tools.Parse("@badges=subscriber/24,bits/1000;color=#00380B;display-name=こにゃった;emotes=;id=a49d99cd-b198-4598-be61-83c51fed6676;mod=0;room-id=39587048;sent-ts=1518059637527;subscriber=1;tmi-sent-ts=1518059639596;turbo=0;user-id=25317975;user-type= :konyatta!konyatta@konyatta.tmi.twitch.tv PRIVMSG #mimicchi :小便まみれの信雄ハウス。");
            return;
        }
        [Test]
        public void ParseTest()
        {
            var result = Tools.Parse("@badges=subscriber/24,bits/1000;color=#00380B;display-name=こにゃった;emotes=;id=a49d99cd-b198-4598-be61-83c51fed6676;mod=0;room-id=39587048;sent-ts=1518059637527;subscriber=1;tmi-sent-ts=1518059639596;turbo=0;user-id=25317975;user-type= :konyatta!konyatta@konyatta.tmi.twitch.tv PRIVMSG #mimicchi :小便まみれの信雄ハウス。");
            //TODO:
        }
        [Test]
        public void GetChannelNameTest()
        {
            Assert.AreEqual("#ryu123", Tools.GetChannelName("ryu123"));
            Assert.AreEqual("#ryu123", Tools.GetChannelName("https://www.twitch.tv/ryu123"));
            Assert.AreEqual("#ryu123", Tools.GetChannelName("https://www.twitch.tv/ryu123?abc"));
            Assert.Throws<ArgumentException>(() => Tools.GetChannelName("test.net/ryu123"));
        }
        [Test]
        public void ExtractCookiesTest()
        {
            var cookie = new Cookie("a", "b") { Domain = "int-main.net", Path = "/" };
            var cc = new CookieContainer();
            cc.Add(cookie);
            var list = Tools.ExtractCookies(cc);
            Assert.AreEqual("a", list[0].Name);
            Assert.AreEqual("b", list[0].Value);
            Assert.AreEqual("int-main.net", list[0].Domain);
            Assert.AreEqual("/", list[0].Path);
        }
        [Test]
        public void GetRandomGuestUsernameTest()
        {
            var s = Tools.GetRandomGuestUsername();
            Assert.IsTrue(System.Text.RegularExpressions.Regex.IsMatch(s, "justinfan\\d+"));
        }
    }
}
