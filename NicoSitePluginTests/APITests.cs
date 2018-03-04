using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Moq;
using NicoSitePlugin.Old;
using System.Net;
using System.Xml.Serialization;
using System.IO;

namespace NicoSitePluginTests
{
    [TestFixture]
    class APITests
    {
        const string _heartbeatOk = "<?xml version=\"1.0\" encoding=\"utf-8\"?><heartbeat status=\"ok\" time=\"1520133506\"><watchCount>3092</watchCount><commentCount>2180</commentCount><is_restrict></is_restrict><ticket>2297426:lv311467561:0:1520133506:77190867c31df2f6</ticket><waitTime>60</waitTime></heartbeat>";
        const string _heartbeartFail = "<?xml version=\"1.0\" encoding=\"utf-8\"?><heartbeat status=\"fail\" time=\"1520135935\"><error><code>NOTFOUND_STREAM</code><description>stream not found</description><reject>true</reject></error></heartbeat>";
        [Test]
        public async Task Nico_HeartbeatOkTest()
        {
            var ccMock = new Mock<CookieContainer>();
            var dataSourceMock = new Mock<IDataSource>();
            dataSourceMock.Setup(m => m.Get("http://live.nicovideo.jp/api/heartbeat?v=lv311467561", ccMock.Object)).ReturnsAsync(_heartbeatOk);

            var res = await API.GetHeartbeatAsync(dataSourceMock.Object, "lv311467561", ccMock.Object);
            Assert.IsTrue(res.Success);
            var heartbeat = res.Heartbeat;
            Assert.AreEqual("60", heartbeat.WaitTime);
        }
        [Test]
        public async Task Nico_HeartbeatFailTest()
        {
            var ccMock = new Mock<CookieContainer>();
            var dataSourceMock = new Mock<IDataSource>();
            dataSourceMock.Setup(m => m.Get("http://live.nicovideo.jp/api/heartbeat?v=lv311467561", ccMock.Object)).ReturnsAsync(_heartbeartFail);

            var res = await API.GetHeartbeatAsync(dataSourceMock.Object, "lv311467561", ccMock.Object);
            Assert.IsFalse(res.Success);
            var fail = res.Fail;
            Assert.AreEqual("NOTFOUND_STREAM", fail.Code);
        }
    }
}
