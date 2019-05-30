using NicoSitePlugin.Websocket;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NicoSitePluginTests
{
    [TestFixture]
    class MessageParserTests
    {
        MessageParser _parser;
        [SetUp]
        public void SetUp()
        {
            _parser = new MessageParser();
        }
        [Test]
        public void CurrentRoomParseTest()
        {
            var data = "{\"type\":\"watch\",\"body\":{\"room\":{\"messageServerUri\":\"wss://msg.live2.nicovideo.jp/u10199/websocket\",\"messageServerType\":\"niwavided\",\"roomName\":\"co3860652\",\"threadId\":\"1651504725\",\"forks\":[0],\"importedForks\":[]},\"command\":\"currentroom\"}}";
            var parser = new MessageParser();
            var internalMessage = parser.Parse(data);
            var currentRoom = internalMessage as ICurrentRoom;
            Assert.AreEqual("niwavided", currentRoom.MessageServerType);
            Assert.AreEqual("wss://msg.live2.nicovideo.jp/u10199/websocket", currentRoom.MessageServerUri);
            Assert.AreEqual("co3860652", currentRoom.RoomName);
            Assert.AreEqual("1651504725", currentRoom.ThreadId);
        }
    }
}
