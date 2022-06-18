using NUnit.Framework;
using ShowRoomSitePlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowRoomSitePluginTests
{
    [TestFixture]
    public class MessageParserTests
    {
        [Test]
        public void PingParseTest()
        {
            var data = "PING\tshowroom";
            var internalMessage = MessageParser.Parse(data) as Ping;
            Assert.IsNotNull(internalMessage);
        }
        [Test]
        public void PongParseTest()
        {
            var data = "PONG\tshowroom";
            var internalMessage = MessageParser.Parse(data) as Pong;
            Assert.IsNotNull(internalMessage);
        }
        [Test]
        public void Type1ParseTest()
        {
            var data = "MSG\t6cda70:87HHYS8k\t{\"av\":1014474,\"d\":0,\"ac\":\"しまやん♥\",\"cm\":\"マイクが小さい\",\"u\":2370410,\"created_at\":1561880210,\"at\":0,\"t\":\"1\"}";
            var internalMessage = MessageParser.Parse(data) as T1;
            Assert.IsNotNull(internalMessage);
            Assert.AreEqual("しまやん♥", internalMessage.UserName);
            Assert.AreEqual("マイクが小さい", internalMessage.Comment);
            Assert.AreEqual(1561880210, internalMessage.CreatedAt);
            Assert.AreEqual(InternalMessageType.t1, internalMessage.MessageType);
            Assert.AreEqual(data, internalMessage.Raw);
            Assert.AreEqual(2370410, internalMessage.UserId);
        }
        [Test]
        public void Type2ParseTest()
        {
            var data = "MSG\t6cda70:87HHYS8k\t{\"n\":10,\"av\":1001422,\"d\":8,\"ac\":\"マカオのりゅう\",\"created_at\":1561880211,\"u\":842213,\"h\":0,\"g\":1001,\"gt\":2,\"at\":0,\"t\":\"2\"}";
            var internalMessage = MessageParser.Parse(data) as T2;
            Assert.IsNotNull(internalMessage);
            Assert.AreEqual("マカオのりゅう", internalMessage.Ac);
            Assert.AreEqual(0, internalMessage.At);
            Assert.AreEqual(1001422, internalMessage.Av);

            Assert.AreEqual(1561880211, internalMessage.CreatedAt);
            Assert.AreEqual(8, internalMessage.D);
            Assert.AreEqual(1001, internalMessage.G);
            Assert.AreEqual(2, internalMessage.Gt);
            Assert.AreEqual(0, internalMessage.H);
            Assert.AreEqual(10, internalMessage.N);
            Assert.AreEqual(InternalMessageType.t2, internalMessage.MessageType);
            Assert.AreEqual(data, internalMessage.Raw);
            Assert.AreEqual(842213, internalMessage.U);
        }
    }
}
