using TwitchSitePlugin;
using NUnit.Framework;
using SitePlugin;
using System.Linq;
using System.Collections.Generic;
namespace TwitchSitePluginTests
{

    [TestFixture]
    public class CommentViewModelTests
    {
        [Test]
        public void Twitch_PrivmsgParseTest2()
        {
            var message = "@badges=subscriber/12,partner/1;color=#FF0000;display-name=harutomaru;emotes=189031:20-31,60-71/588715:33-58/635709:73-82;id=9029a587-81b0-4705-8607-38cba9b762d6;mod=0;room-id=39587048;subscriber=1;tmi-sent-ts=1518062412116;turbo=0;user-id=72777405;user-type= :harutomaru!harutomaru@harutomaru.tmi.twitch.tv PRIVMSG #mimicchi :@bwscar221 おざまぁぁぁす！ mimicchiHage haruto1Harutomarubakayarou mimicchiHage bwscarDead";
            var actual = Tools.GetMessageItems(Tools.Parse(message));

            var expected = new List<IMessagePart>
            {
                new MessageText("@bwscar221 おざまぁぁぁす！ "),
                new MessageImage{Url="https://static-cdn.jtvnw.net/emoticons/v1/189031/1.0", Alt="mimicchiHage"},
                new MessageText(" "),
                new MessageImage{Url="https://static-cdn.jtvnw.net/emoticons/v1/588715/1.0", Alt="haruto1Harutomarubakayarou"},
                new MessageText(" "),
                new MessageImage{Url="https://static-cdn.jtvnw.net/emoticons/v1/189031/1.0", Alt="mimicchiHage"},
                new MessageText(" "),
                new MessageImage{Url="https://static-cdn.jtvnw.net/emoticons/v1/635709/1.0", Alt="bwscarDead"},
            };
            CollectionAssert.AreEquivalent(expected, actual);
        }
        [Test]
        public void Twitch_PrivmsgParseTest1()
        {
            var s = "@badges=moderator/1,subscriber/24,twitchcon2017/1;color=#8A2BE2;display-name=あらさん;emotes=299516:10-25,27-42,44-59;id=a873029f-0979-4e29-8da4-65e980f4bc6e;mod=1;room-id=39587048;subscriber=1;tmi-sent-ts=1518062640494;turbo=0;user-id=49418165;user-type=mod :takutakubanban!takutakubanban@takutakubanban.tmi.twitch.tv PRIVMSG #mimicchi :いぐすりいいぞ！！ mimicchiOjiichan mimicchiOjiichan mimicchiOjiichan";
            var actual = Tools.GetMessageItems(Tools.Parse(s));
            var expected = new List<IMessagePart>
            {
                new MessageText("いぐすりいいぞ！！ "),
                new MessageImage{ Url = "https://static-cdn.jtvnw.net/emoticons/v1/299516/1.0", Alt="mimicchiOjiichan"},
                new MessageText(" "),
                new MessageImage{ Url = "https://static-cdn.jtvnw.net/emoticons/v1/299516/1.0", Alt="mimicchiOjiichan"},
                new MessageText(" "),
                new MessageImage{ Url = "https://static-cdn.jtvnw.net/emoticons/v1/299516/1.0", Alt="mimicchiOjiichan"},
            };
            CollectionAssert.AreEquivalent(expected, actual);
        }
        [Test]
        public void Twitch_PrivmsgParseTest0()
        {
            var s = "@badges=moderator/1,subscriber/12;color=#008000;display-name=かみっち;emotes=;id=e19ddbbe-20d6-46ed-8ebd-a9022da86646;mod=1;room-id=68333687;subscriber=1;tmi-sent-ts=1518074694743;turbo=0;user-id=55167254;user-type=mod :shellz_kami!shellz_kami@shellz_kami.tmi.twitch.tv PRIVMSG #ohirun :出るんだとしたら２０周年が丁度いいんじゃないかなって";
            var actual = Tools.GetMessageItems(Tools.Parse(s));
            var expected = new List<IMessagePart>
            {
                new MessageText("出るんだとしたら２０周年が丁度いいんじゃないかなって"),
            };
            CollectionAssert.AreEquivalent(expected, actual);
        }
    }
}
