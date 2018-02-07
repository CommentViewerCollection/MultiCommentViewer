using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchSitePlugin;
using NUnit.Framework;
namespace TwitchSitePluginTests
{
    [TestFixture]
    public class ToolsTests
    {
        [Test]
        public void GetRandomGuestUsernameTest()
        {
            var s = Tools.GetRandomGuestUsername();
            Assert.IsTrue(System.Text.RegularExpressions.Regex.IsMatch(s, "justinfan\\d+"));
        }
    }
    [TestFixture]
    public class CommandParseTests
    {
        [Test]
        public void TwitchCommand001ParseTest()
        {
            var s = ":tmi.twitch.tv 001 justinfan12345 :Welcome, GLHF!";
            var result = Tools.Parse(s);
            Assert.AreEqual("tmi.twitch.tv", result.Prefix);
            Assert.AreEqual("001", result.Command);
            CollectionAssert.AreEquivalent(new List<string> { "justinfan12345", "Welcome, GLHF!" }, result.Params);
        }
        [Test]
        public void TwitchCommand002ParseTest()
        {
            var s = ":tmi.twitch.tv 002 justinfan12345 :Your host is tmi.twitch.tv";
            var result = Tools.Parse(s);
            Assert.AreEqual("tmi.twitch.tv", result.Prefix);
            Assert.AreEqual("002", result.Command);
            CollectionAssert.AreEquivalent(new List<string> { "justinfan12345", "Your host is tmi.twitch.tv" }, result.Params);
        }
        [Test]
        public void TwitchCommand003ParseTest()
        {
            var s = ":tmi.twitch.tv 003 justinfan12345 :This server is rather new";
            var result = Tools.Parse(s);
            Assert.AreEqual("tmi.twitch.tv", result.Prefix);
            Assert.AreEqual("003", result.Command);
            CollectionAssert.AreEquivalent(new List<string> { "justinfan12345", "This server is rather new" }, result.Params);
        }
        [Test]
        public void TwitchCommand004ParseTest()
        {
            var s = ":tmi.twitch.tv 004 justinfan12345 :-";
            var result = Tools.Parse(s);
            Assert.AreEqual("tmi.twitch.tv", result.Prefix);
            Assert.AreEqual("004", result.Command);
            CollectionAssert.AreEquivalent(new List<string> { "justinfan12345", "-" }, result.Params);
        }
        [Test]
        public void TwitchCommand353ParseTest()
        {
            var s = ":justinfan12345.tmi.twitch.tv 353 justinfan12345 = #gugu2525 :justinfan12345";
            var result = Tools.Parse(s);
            Assert.AreEqual("justinfan12345.tmi.twitch.tv", result.Prefix);
            Assert.AreEqual("353", result.Command);
            CollectionAssert.AreEquivalent(new List<string> { "justinfan12345", "=", "#gugu2525", "justinfan12345" }, result.Params);
        }
        [Test]
        public void TwitchCommand366ParseTest()
        {
            var s = ":justinfan12345.tmi.twitch.tv 366 justinfan12345 #gugu2525 :End of /NAMES list";
            var result = Tools.Parse(s);
            Assert.AreEqual("justinfan12345.tmi.twitch.tv", result.Prefix);
            Assert.AreEqual("366", result.Command);
            CollectionAssert.AreEquivalent(new List<string> { "justinfan12345", "#gugu2525", "End of /NAMES list" }, result.Params);
        }
        [Test]
        public void TwitchCommand372ParseTest()
        {
            var s = ":tmi.twitch.tv 372 justinfan12345 :You are in a maze of twisty passages, all alike.";
            var result = Tools.Parse(s);
            Assert.AreEqual("tmi.twitch.tv", result.Prefix);
            Assert.AreEqual("372", result.Command);
            CollectionAssert.AreEquivalent(new List<string> { "justinfan12345", "You are in a maze of twisty passages, all alike." }, result.Params);
        }
        [Test]
        public void TwitchCommand375ParseTest()
        {
            var s = ":tmi.twitch.tv 375 justinfan12345 :-";
            var result = Tools.Parse(s);
            Assert.AreEqual("tmi.twitch.tv", result.Prefix);
            Assert.AreEqual("375", result.Command);
            CollectionAssert.AreEquivalent(new List<string> { "justinfan12345", "-" }, result.Params);
        }
        [Test]
        public void TwitchCommand376ParseTest()
        {
            var s = ":tmi.twitch.tv 376 justinfan12345 :>";
            var result = Tools.Parse(s);
            Assert.AreEqual("tmi.twitch.tv", result.Prefix);
            Assert.AreEqual("376", result.Command);
            CollectionAssert.AreEquivalent(new List<string> { "justinfan12345", ">" }, result.Params);
        }
        [Test]
        public void TwitchCommandJOINParseTest()
        {
            var s = ":justinfan12345!justinfan12345@justinfan12345.tmi.twitch.tv JOIN #gugu2525";
            var result = Tools.Parse(s);
            Assert.AreEqual("justinfan12345!justinfan12345@justinfan12345.tmi.twitch.tv", result.Prefix);
            Assert.AreEqual("JOIN", result.Command);
            CollectionAssert.AreEquivalent(new List<string> { "#gugu2525" }, result.Params);
        }
        //@badges=premium/1;color=;display-name=clllp;emotes=;id=3c18fda8-8008-460d-9442-ca4451b5fb84;mod=0;room-id=95066484;subscriber=0;tmi-sent-ts=1517933598965;turbo=0;user-id=156711248;user-type= :clllp!clllp@clllp.tmi.twitch.tv PRIVMSG #gugu2525 :意識過剰もほどほどに
        [Test]
        public void TwitchCommandPRIVMSGParseTest()
        {
            var s = "@badges=premium/1;color=;display-name=clllp;emotes=;id=3c18fda8-8008-460d-9442-ca4451b5fb84;mod=0;room-id=95066484;subscriber=0;tmi-sent-ts=1517933598965;turbo=0;user-id=156711248;user-type= :clllp!clllp@clllp.tmi.twitch.tv PRIVMSG #gugu2525 :意識過剰もほどほどに";
            var result = Tools.Parse(s);
            CollectionAssert.AreEquivalent(new Dictionary<string, string>
            {
                {"badges","premium/1" },
                {"color","" },
                {"display-name","clllp" },
                { "emotes", ""},
                { "id","3c18fda8-8008-460d-9442-ca4451b5fb84"},
                { "mod","0"},
                { "room-id","95066484"},
                { "subscriber","0"},
                { "tmi-sent-ts","1517933598965"},
                { "turbo","0"},
                { "user-id","156711248"},
                { "user-type",""},
            }, result.Tags);
            Assert.AreEqual("clllp!clllp@clllp.tmi.twitch.tv", result.Prefix);
            Assert.AreEqual("PRIVMSG", result.Command);
            CollectionAssert.AreEquivalent(new List<string> { "#gugu2525", "意識過剰もほどほどに" }, result.Params);
        }
        [Test]
        public void TwitchCommandROOMSTATEParseTest()
        {
            var s = "@broadcaster-lang=ja;emote-only=0;followers-only=-1;mercury=0;r9k=0;rituals=0;room-id=95066484;slow=0;subs-only=0 :tmi.twitch.tv ROOMSTATE #gugu2525";
            var result = Tools.Parse(s);

            var dict = new Dictionary<string, string>
            {
                {"broadcaster-lang","ja" },
                {"emote-only","0" },
                { "followers-only","-1" },
                { "mercury","0" },
                { "r9k","0" },
                { "rituals","0" },
                { "room-id","95066484" },
                {"slow","0" },
                { "subs-only","0"},
            };
            CollectionAssert.AreEquivalent(dict, result.Tags);
            Assert.AreEqual("tmi.twitch.tv", result.Prefix);
            Assert.AreEqual("ROOMSTATE", result.Command);
            CollectionAssert.AreEquivalent(new List<string> { "#gugu2525" }, result.Params);
        }
    }
}
