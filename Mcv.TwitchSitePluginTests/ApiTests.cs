using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchSitePlugin;

namespace TwitchSitePluginTests
{
    [TestFixture]
    class ApiTests
    {
        [Test]
        public async Task GetEmoticonsTest()
        {
            //var serverMock = new Mock<IDataServer>();
            //serverMock.Setup(s => s.GetAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).Returns(Task.FromResult(data));
            //var server = serverMock.Object;
            //await API.GetEmoticons(new TwitchServer());
        }
        [Test]
        public async Task GetChannelProductsTest()
        {
            var data = TestHelper.GetSampleData("ChannelProduct.txt");
            var serverMock = new Mock<IDataServer>();
            serverMock.Setup(s => s.GetAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).Returns(Task.FromResult(data));
            var server = serverMock.Object;
            var products = await API.GetChannelProducts(server, "ukyochi_jp");
        }
        [Test]
        public async Task GetStreamAsyncTest()
        {
            var data = TestHelper.GetSampleData("Streams.txt");
            var serverMock = new Mock<IDataServer>();
            serverMock.Setup(s => s.GetAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).Returns(Task.FromResult(data));
            var server = serverMock.Object;
            var stream=await API.GetStreamAsync(server, "ukyochi_jp");
            Assert.AreEqual("Shanghai, CHINA - STUFF w/ !Water jnbShiba - !Schedule !Jake !Discord !YouTube - Follow @JakenbakeLIVE", stream.Title);
            Assert.AreEqual("32961080624", stream.LiveId);
            Assert.AreEqual("JakenbakeLIVE", stream.Username);
            Assert.AreEqual("live", stream.Type);
        }
        [Test]
        public async Task GetStreamAsyncEmptyTest()
        {
            var data = "{\"data\":[],\"pagination\":{}}";
            var serverMock = new Mock<IDataServer>();
            serverMock.Setup(s => s.GetAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).Returns(Task.FromResult(data));
            var server = serverMock.Object;
            var stream = await API.GetStreamAsync(server, "ukyochi_jp");
            Assert.IsNull(stream);
        }
    }
}
