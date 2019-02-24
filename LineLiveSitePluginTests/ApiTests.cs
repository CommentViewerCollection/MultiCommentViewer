using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using LineLiveSitePlugin;
using Moq;

namespace LineLiveSitePluginTests
{
    [TestFixture]
    class ApiTests
    {
        [Test]
        public async Task GetPromptyStatsTest()
        {
            var data = TestHelper.GetSampleData("PromptyStats.txt");
            var serverMock = new Mock<IDataServer>();
            serverMock.Setup(s => s.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(data));
            var server = serverMock.Object;
            var res = await Api.GetPromptyStats(server, "", "");
            Assert.AreEqual("LIVE", res.LiveStatus);
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual(200, res.ApiStatusCode);
        }
    }
}
