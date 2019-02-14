using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using YouTubeLiveSitePlugin.Test2;
namespace YouTubeLiveSitePluginTests
{
    [TestFixture]
    class VidResolverTests
    {
        [Test]
        public async Task ResolveVidFromWatchUrl()
        {
            var s = new VidResolver();
            var serverMock = new Mock<IYouTubeLibeServer>();
            var result1 = await s.GetVid(serverMock.Object, "https://www.youtube.com/watch?v=Rs-WxTGgVus");
            Assert.IsTrue(result1 is IVidResult);
            Assert.AreEqual("Rs-WxTGgVus", ((VidResult)result1).Vid);

            var result2 = await s.GetVid(serverMock.Object, "https://www.youtube.com/watch?v=Rs-WxTGgVus&feature=test");
            Assert.IsTrue(result2 is IVidResult);
            Assert.AreEqual("Rs-WxTGgVus", ((VidResult)result2).Vid);
        }
        [Test]
        public async Task ResolveVidFromChannelUrl()
        {
            var sample = Tools.GetSampleData("Channel_some_archives.txt");            
            var s = new VidResolver();
            var serverMock = new Mock<IYouTubeLibeServer>();
            serverMock.Setup(k => k.GetEnAsync("https://www.youtube.com/channel/UCkDuaqQxw3k7Aa816kngUYg/videos?flow=list&view=2")).Returns(Task.FromResult(sample));
            var result1 = await s.GetVid(serverMock.Object, "https://www.youtube.com/channel/UCkDuaqQxw3k7Aa816kngUYg");
            Assert.IsTrue(result1 is NoVidResult);
        }
        [Test]
        public async Task GetVidsFromChannelId_CC_label_Test()
        {
            var html = Tools.GetSampleData("Channel_some_archives_with_cc_label.txt");
            var server = new Mock<IYouTubeLibeServer>();
            var channelId = "UCBL9Blq9GDhPGAQbfUJIYXQ";
            server.Setup(s => s.GetEnAsync("https://www.youtube.com/channel/" + channelId + "/videos?flow=list&view=2")).Returns(Task.FromResult(html));
            var resolver = new VidResolver();
            var vids = await resolver.GetVidsFromChannelId(server.Object, channelId);
            Assert.IsTrue(vids.Count == 0);
        }
        [Test]
        public async Task GetVidsFromChannelId_on_air_Test()
        {
            var html = Tools.GetSampleData("Channel_on_air.txt");
            var server = new Mock<IYouTubeLibeServer>();
            var channelId = "UCBL9Blq9GDhPGAQbfUJIYXQ";
            server.Setup(s => s.GetEnAsync("https://www.youtube.com/channel/" + channelId + "/videos?flow=list&view=2")).Returns(Task.FromResult(html));
            var resolver = new VidResolver();
            var vids = await resolver.GetVidsFromChannelId(server.Object, channelId);
            Assert.IsTrue(vids.Count == 1);
            Assert.AreEqual("AuFOOUtIyUY", vids[0]);
        }
        [Test]
        public async Task GetVidsFromChannelId_Three_on_air_Test()
        {
            var html = Tools.GetSampleData("Channel_Three_on_air.txt");
            var server = new Mock<IYouTubeLibeServer>();
            var channelId = "UCBL9Blq9GDhPGAQbfUJIYXQ";
            server.Setup(s => s.GetEnAsync("https://www.youtube.com/channel/" + channelId + "/videos?flow=list&view=2")).Returns(Task.FromResult(html));
            var resolver = new VidResolver();
            var vids = await resolver.GetVidsFromChannelId(server.Object, channelId);
            Assert.IsTrue(vids.Count == 3);
            Assert.AreEqual("Vls4h1GAP-c", vids[0]);
            Assert.AreEqual("hUjRuVhJ_4o", vids[1]);
            Assert.AreEqual("2ccaHpy5Ewo", vids[2]);

        }
    }
}
