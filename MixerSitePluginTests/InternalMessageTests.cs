using MixerSitePlugin;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixerSitePluginTests
{
    [TestFixture]
    class InternalMessageTests
    {
        [Test]
        public void DeleteMessageEventのRawが正しく機能するか()
        {
            //origin.Rawがテスト対象
            //origin.Rawをパーサに渡して正しいDeleteMessageEventが生成されるか確認する
            var origin = new DeleteMessageEvent("123", new User("name", 229, new[] { "role1", "role2" }, 998));
            var parsed = InternalMessageParser.Parse(origin.Raw, null) as DeleteMessageEvent;
            
            Assert.AreEqual(origin.Moderator.UserId, parsed.Moderator.UserId);
            Assert.AreEqual(origin.Moderator.UserName, parsed.Moderator.UserName);
            Assert.AreEqual(origin.Moderator.UserLevel, parsed.Moderator.UserLevel);
            CollectionAssert.AreEqual(origin.Moderator.UserRoles, parsed.Moderator.UserRoles);
            Assert.AreEqual(origin.Id, parsed.Id);
            Assert.AreEqual(origin.Raw, parsed.Raw);
        }
        [Test]
        public void OptOutEventsMethodのRawが正しく機能するか()
        {
            //origin.Rawがテスト対象
            //origin.Rawをパーサに渡して正しいOptOutEventsMethodが生成されるか確認する
            var origin = new OptOutEventsMethod(99, new string[] { "a", "b" });
            var parsed = InternalMessageParser.Parse(origin.Raw, null) as OptOutEventsMethod;

            Assert.AreEqual(origin.Id, parsed.Id);
            Assert.AreEqual(origin.Raw, parsed.Raw);
            CollectionAssert.AreEqual(origin.Arguments, parsed.Arguments);
        }
        [Test]
        public void AuthMethodのRawが正しく機能するか()
        {
            //origin.Rawがテスト対象
            //origin.Rawをパーサに渡して正しいAuthMethodが生成されるか確認する
            var origin = new AuthMethod(99, 123);
            var parsed = InternalMessageParser.Parse(origin.Raw, null) as AuthMethod;

            Assert.AreEqual(origin.ChannelId, parsed.ChannelId);
            Assert.AreEqual(origin.Id, parsed.Id);
            Assert.AreEqual(origin.MyId, parsed.MyId);
            Assert.AreEqual(origin.Raw, parsed.Raw);
            Assert.AreEqual(origin.Token, parsed.Token);
        }
    }
}
