using MildomSitePlugin;
using NUnit.Framework;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MildomSitePluginTests
{
    [TestFixture]
    class Class2
    {
        [Test]
        public void Test()
        {
            var data = "{\"area\": 2000, \"cmd\": \"onChat\", \"fansBgPic\": null, \"fansGroupType\": null, \"fansLevel\": null, \"fansName\": null, \"level\": 7, \"medals\": null, \"msg\": \"あ[/1001][/1002]い\", \"reqId\": 0, \"roomAdmin\": 0, \"roomId\": 10038336, \"toId\": 10038336, \"toName\": \"a\", \"type\": 3, \"userId\": 10088625, \"userImg\": \"\", \"userName\": \"kkkkk\"}";
            var chat = MessageParser.Parse(data, new Dictionary<int, string>
            {
                { 1001, "abc" },
                { 1002, "xyz" },
            }) as OnChatMessage;
            var l = chat.MessageItems.ToList();
            Assert.AreEqual(Common.MessagePartFactory.CreateMessageText("あ"), l[0]);
            Assert.AreEqual("abc", (l[1] as IMessageImage).Url);
            Assert.AreEqual("xyz", (l[2] as IMessageImage).Url);
            Assert.AreEqual(Common.MessagePartFactory.CreateMessageText("い"), l[3]);
        }
        [Test]
        public void userIdが無いonChat()
        {
            var data = "{\"area\": 2000, \"cmd\": \"onChat\", \"msg\": \"よくやった\", \"msgId\": \"1598498460835_0_8192\", \"reqId\": 0, \"roomId\": 10007428, \"time\": \"1598498460835\", \"toId\": 10007428, \"toName\": \"*\", \"type\": 3, \"userName\": \"guest737168\"}";
            var chat = MessageParser.Parse(data, new Dictionary<int, string>()) as OnChatMessage;
            Assert.IsNull(chat.UserId);
            Assert.IsNull(chat.UserImg);
        }
        [Test]
        public void 匿名ユーザーのonAdd()
        {
            var data = "{\"area\": 1000, \"avatarDecortaion\": 0, \"cmd\": \"onAdd\", \"enterroomEffect\": 0, \"isFirstTopup\": null, \"level\": 1, \"loveCountSum\": 0, \"medals\": null, \"nobleLevel\": 0, \"reqId\": 0, \"roomId\": 10592943, \"rst\": 0, \"type\": 3, \"userCount\": 369, \"userId\": 0, \"userImg\": null, \"userName\": \"guest101168\"}";
            var onAdd = MessageParser.Parse(data, new Dictionary<int, string>()) as OnAddMessage;
            Assert.AreEqual("guest101168", onAdd.UserId);
        }
    }
}
