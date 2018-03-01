using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NicoSitePlugin.Websocket;
using NUnit.Framework;

namespace NicoSitePluginTests
{
    [TestFixture]
    public class MetadataParserTests
    {
        MessageParser _parser = new MessageParser();
        [Test]
        public void ServertimeParseTest()
        {
            var s = "{\"type\":\"watch\",\"body\":{\"command\":\"servertime\",\"params\":[\"1517720296543\"]}}";
            var message = _parser.Parse(s);
            if (message is IServertime servertime)
            {
                Assert.AreEqual("1517720296543", servertime.Time);
                Assert.AreEqual(servertime.Raw, s);
            }
            else
            {
                Assert.Fail();
            }
        }
        [Test]
        public void CurrentRoomParseTest()
        {
            var s = "{\"type\":\"watch\",\"body\":{\"room\":{\"messageServerUri\":\"ws://msg105.live.nicovideo.jp:99/websocket\",\"messageServerType\":\"niwavided\",\"roomName\":\"立ち見D列\",\"threadId\":\"1620962125\",\"forks\":[0],\"importedForks\":[]},\"command\":\"currentroom\"}}";
            var message = _parser.Parse(s);
            if (message is ICurrentRoom currentRoom)
            {
                Assert.AreEqual("ws://msg105.live.nicovideo.jp:99/websocket", currentRoom.MessageServerUri);
                Assert.AreEqual("niwavided", currentRoom.MessageServerType);
                Assert.AreEqual("立ち見D列", currentRoom.RoomName);
                Assert.AreEqual("1620962125", currentRoom.ThreadId);
                Assert.AreEqual(currentRoom.Raw, s);
            }
            else
            {
                Assert.Fail();
            }
        }
        [Test]
        public void PermitParseTest()
        {
            var s = "{\"type\":\"watch\",\"body\":{\"command\":\"permit\",\"params\":[\"7545090474615\"]}}";

            var message = _parser.Parse(s);
            if (message is IPermit permit)
            {
                Assert.AreEqual("7545090474615", permit.Value);
                Assert.AreEqual(permit.Raw, s);
            }
            else
            {
                Assert.Fail();
            }
        }
        [Test]
        public void ScheduleParseTest()
        {
            var s = "{\"type\":\"watch\",\"body\":{\"update\":{\"begintime\":1517713200000,\"endtime\":1517734800000},\"command\":\"schedule\"}}";

            var message = _parser.Parse(s);
            if (message is ISchedule schedule)
            {
                Assert.AreEqual(1517713200000, schedule.BeginTime);
                Assert.AreEqual(1517734800000, schedule.EndTime);
                Assert.AreEqual(schedule.Raw, s);
            }
            else
            {
                Assert.Fail();
            }
        }
        [Test]
        public void StatisticsParseTest()
        {
            var s = "{\"type\":\"watch\",\"body\":{\"command\":\"statistics\",\"params\":[\"8379\",\"8171\",\"0\",\"0\"]}}";
            var message = _parser.Parse(s);
            if (message is IStatistics statistics)
            {
                Assert.AreEqual("8379", statistics.ViewerCount);
                Assert.AreEqual("8171", statistics.CommentCount);
                Assert.AreEqual(statistics.Raw, s);
            }
            else
            {
                Assert.Fail();
            }
        }
        [Test]
        public void PingParseTest()
        {
            var s = "{\"type\":\"ping\",\"body\":{}}";
            var message = _parser.Parse(s);
            Assert.IsTrue(message is IPing);
        }
    }
}
