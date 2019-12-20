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
    }
}
