﻿using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using YouTubeLiveSitePlugin.Input;
using YouTubeLiveSitePlugin.Test2;
namespace YouTubeLiveSitePluginTests
{
    [TestFixture]
    class ChannelLiveResearcherTests
    {
        [Test]
        public async Task GetVidsAsyncLiveNowTest()
        {
            var data = Tools.GetSampleData("Channel_LiveNow_selected.txt");
            var serverMock = new Mock<IYouTubeLibeServer>();
            serverMock.Setup(s => s.GetEnAsync(It.IsAny<string>())).Returns(Task.FromResult(data));
            var a = await ChannelLiveResearcher.GetVidsAsync(serverMock.Object, "channelid");
            Assert.AreEqual("GnW76d1A3YQ", a[0]);
        }
        [Test]
        public async Task GetVidsAsyncLiveNow2LivesTest()
        {
            var data = Tools.GetSampleData("Channel_LiveNow_selected_2lives.txt");
            var serverMock = new Mock<IYouTubeLibeServer>();
            serverMock.Setup(s => s.GetEnAsync(It.IsAny<string>())).Returns(Task.FromResult(data));
            var a = await ChannelLiveResearcher.GetVidsAsync(serverMock.Object, "channelid");
            Assert.AreEqual("f0CxUscMX20", a[0]);
            Assert.AreEqual("EHkMjfMw7oU", a[1]);
        }
    }
    [TestFixture]
    class VidResolverTests
    {
        [Test]
        public async Task 短縮URLに対応()
        {
            var s = new VidResolver();
            var serverMock = new Mock<IYouTubeLibeServer>();
            var result1 = await s.GetVid(serverMock.Object, new WatchUrl("https://youtu.be/bexmlC2nD0U"));
            Assert.IsTrue(result1 is IVidResult);
            Assert.AreEqual("bexmlC2nD0U", ((VidResult)result1).Vid);
        }
        [Test]
        public async Task ResolveVidFromWatchUrl()
        {
            var s = new VidResolver();
            var serverMock = new Mock<IYouTubeLibeServer>();
            var result1 = await s.GetVid(serverMock.Object, new WatchUrl("https://www.youtube.com/watch?v=Rs-WxTGgVus"));
            Assert.IsTrue(result1 is IVidResult);
            Assert.AreEqual("Rs-WxTGgVus", ((VidResult)result1).Vid);

            var result2 = await s.GetVid(serverMock.Object, new WatchUrl("https://www.youtube.com/watch?v=Rs-WxTGgVus&feature=test"));
            Assert.IsTrue(result2 is IVidResult);
            Assert.AreEqual("Rs-WxTGgVus", ((VidResult)result2).Vid);
        }
        [Test]
        public async Task ResolveVidFromChanneLivelUrl()
        {
            var sample = Tools.GetSampleData("Channel_live.txt");
            var s = new VidResolver();
            var serverMock = new Mock<IYouTubeLibeServer>();
            serverMock.Setup(k => k.GetEnAsync(It.IsAny<string>())).Returns(Task.FromResult(sample));
            var result1 = await s.GetVid(serverMock.Object, new ChannelUrl("https://www.youtube.com/channel/UCkDuaqQxw3k7Aa816kngUYg")) as VidResult;
            Assert.IsTrue(result1 is VidResult);
            Assert.AreEqual("klvzbBP7zM8", ((VidResult)result1).Vid);
        }
        //[Test]
        //public async Task ResolveVidFromChannelUrl()
        //{
        //    var sample = Tools.GetSampleData("Channel_some_archives.txt");            
        //    var s = new VidResolver();
        //    var serverMock = new Mock<IYouTubeLibeServer>();
        //    serverMock.Setup(k => k.GetEnAsync("https://www.youtube.com/channel/UCkDuaqQxw3k7Aa816kngUYg/videos?flow=list&view=0")).Returns(Task.FromResult(sample));
        //    var result1 = await s.GetVid(serverMock.Object, "https://www.youtube.com/channel/UCkDuaqQxw3k7Aa816kngUYg");
        //    Assert.IsTrue(result1 is NoVidResult);
        //}
        [Test]
        public async Task GetVidsFromChannelId_CC_label_Test()
        {
            var html = Tools.GetSampleData("Channel_some_archives_with_cc_label.txt");
            var server = new Mock<IYouTubeLibeServer>();
            var channelId = "UCBL9Blq9GDhPGAQbfUJIYXQ";
            server.Setup(s => s.GetEnAsync("https://www.youtube.com/channel/" + channelId + "/videos?flow=list&view=0")).Returns(Task.FromResult(html));
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
            server.Setup(s => s.GetEnAsync("https://www.youtube.com/channel/" + channelId + "/videos?flow=list&view=0")).Returns(Task.FromResult(html));
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
            server.Setup(s => s.GetEnAsync("https://www.youtube.com/channel/" + channelId + "/videos?flow=list&view=0")).Returns(Task.FromResult(html));
            var resolver = new VidResolver();
            var vids = await resolver.GetVidsFromChannelId(server.Object, channelId);
            Assert.IsTrue(vids.Count == 3);
            Assert.AreEqual("Vls4h1GAP-c", vids[0]);
            Assert.AreEqual("hUjRuVhJ_4o", vids[1]);
            Assert.AreEqual("2ccaHpy5Ewo", vids[2]);

        }
    }
}
