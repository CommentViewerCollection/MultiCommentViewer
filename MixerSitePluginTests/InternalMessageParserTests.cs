using MixerSitePlugin;
using Moq;
using NUnit.Framework;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixerSitePluginTests
{
    [TestFixture]
    public class InternalMessageParserTests
    {
        [Test]
        public void ParseNotJsonMessageTest()
        {
            var data = "abc";
            var msg = InternalMessageParser.Parse(data, null) as UnknownMessage;
            Assert.AreEqual(data, msg.Raw);
        }
        [Test]
        public void ParseUnknownFormatMessageTest()
        {
            var data = "{ \"abc\":123 }";
            var msg = InternalMessageParser.Parse(data, null) as UnknownMessage;
            Assert.AreEqual(data, msg.Raw);
        }
        [Test]
        public void ParseUnknownEventMessageTest()
        {
            var data = "{ \"type\":\"event\",\"event\":\"abc\" }";
            var msg = InternalMessageParser.Parse(data, null) as UnknownMessage;
            Assert.AreEqual(data, msg.Raw);
        }
        [Test]
        public void ParseUnknownMethodMessageTest()
        {
            var data = "{ \"type\":\"method\",\"method\":\"abc\" }";
            var msg = InternalMessageParser.Parse(data, null) as UnknownMessage;
            Assert.AreEqual(data, msg.Raw);
        }
        class UnknownMethod : MethodBase
        {
            public UnknownMethod(long id) : base(id)
            {
            }
            public override string Raw { get; }
        }
        [Test]
        public void ParseUnknownReplyMessageTest()
        {
            var id = 123;
            var method = new UnknownMethod(id);
            var dict = new Dictionary<long, MethodBase>()
            {
                { id, method }
            };
            var data = "{ \"type\":\"reply\",\"reply\":\"abc\",\"id\":"+ id +" }";
            var msg = InternalMessageParser.Parse(data, dict) as UnknownMessage;
            Assert.AreEqual(data, msg.Raw);
        }
        [Test]
        public void ParseUnknownTypeMessageTest()
        {
            var data = "{ \"type\":\"123\" }";
            var msg = InternalMessageParser.Parse(data, null) as UnknownMessage;
            Assert.AreEqual(data, msg.Raw);
        }
        [Test]
        public void ParseWelcomeEventTest()
        {
            var data = "{\"type\":\"event\",\"event\":\"WelcomeEvent\",\"data\":{\"server\":\"d5afec05-4e2c-4816-981c-094f04a4d31c\"}}";
            var msg = InternalMessageParser.Parse(data, null) as WelcomeEvent;
            Assert.AreEqual("d5afec05-4e2c-4816-981c-094f04a4d31c", msg.Server);
        }
        [Test]
        public void ParseChatMessageTest()
        {
            var data = "{\"type\":\"event\",\"event\":\"ChatMessage\",\"data\":{\"channel\":160788,\"id\":\"8fcfe1a0-ccc5-11e9-a733-1900adf73a91\",\"user_name\":\"Cat_King_34\",\"user_id\":108942852,\"user_roles\":[\"User\"],\"user_level\":9,\"user_avatar\":null,\"message\":{\"message\":[{\"type\":\"text\",\"data\":\"\",\"text\":\"\"},{\"type\":\"emoticon\",\"source\":\"external\",\"pack\":\"https://uploads.mixer.com/emoticons/nkkpu1fh-160788.png\",\"coords\":{\"x\":0,\"y\":24,\"width\":24,\"height\":24},\"text\":\":mcatHug\"}],\"meta\":{\"shouldDrop\":false}},\"user_ascension_level\":11}}";
            var msg = InternalMessageParser.Parse(data, null) as ChatMessageData;
            Assert.AreEqual(160788, msg.Channel);
            Assert.AreEqual("8fcfe1a0-ccc5-11e9-a733-1900adf73a91", msg.Id);
            Assert.AreEqual("Cat_King_34", msg.UserName);
            Assert.AreEqual(108942852, msg.UserId);
            Assert.AreEqual(new[] { "User" }, msg.UserRoles);
            Assert.AreEqual(9, msg.UserLevel);
            //Assert.IsNull(msg.UserAvatar);
            Assert.AreEqual(new List<IMessagePart>
            {
                Common.MessagePartFactory.CreateMessageText(""),
                new Common.MessageImage
                {
                    Alt = ":mcatHug",
                    Url ="https://uploads.mixer.com/emoticons/nkkpu1fh-160788.png",
                    Width =24,
                    Height =24,
                },
            }, msg.MessageItems);
            Assert.IsFalse(msg.ShouldDrop);

        }
        [TestCase("{\"type\":\"reply\",\"error\":null,\"id\":1,\"data\":{\"authenticated\":true,\"roles\":[\"User\"]}}", 1, true, new string[] { "User" })]
        [TestCase("{\"type\":\"reply\",\"error\":null,\"id\":1,\"data\":{\"authenticated\":false,\"roles\":[]}}", 1, false, new string[] { })]

        public void ParseAuthReplyTest(string data, long id, bool authenticated, string[] roles)
        {
            var method = new AuthMethod(id, 123);
            var dict = new Dictionary<long, MethodBase>()
            {
                { id, method }
            };
            var reply = InternalMessageParser.Parse(data, dict) as AuthReply;
            Assert.AreEqual(id, reply.Id);
            Assert.AreEqual(authenticated, reply.Authenticated);
            Assert.AreEqual(roles, reply.Roles);
        }
        [Test]
        public void ParsePingReplyTest()
        {
            var id = 99;
            var method = new PingMethod(id);
            var dict = new Dictionary<long, MethodBase>()
            {
                { id, method }
            };
            var data = "{\"type\":\"reply\",\"error\":null,\"id\":" + id + "}";
            var reply = InternalMessageParser.Parse(data, dict) as PingReply;
            Assert.AreEqual(id, reply.Id);
        }
        [Test]
        public void ParseOptOutEventsReplyTest()
        {
            var id = 99;
            var method = new OptOutEventsMethod(id, new string[] { });
            var dict = new Dictionary<long, MethodBase>()
            {
                { id, method }
            };
            var data = "{\"type\":\"reply\",\"error\":null,\"id\":" + id + ",\"data\":{}}";
            var reply = InternalMessageParser.Parse(data, dict) as OptOutEventsReply;
            Assert.AreEqual(id, reply.Id);
        }
    }
}
