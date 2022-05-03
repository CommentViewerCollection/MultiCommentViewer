using Mcv.YouTubeLiveSitePlugin;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace YouTubeLiveSitePluginTests
{
    [TestFixture]
    class ChannelLiveFinderTests
    {
        [Test]
        public async Task Tak()
        {
            var data = Tools.GetSampleData("Channel_LiveNow_20221127.txt");
            var serverMock = new Mock<IYouTubeLiveServer>();
            serverMock.Setup(s => s.GetEnAsync(It.IsAny<string>())).Returns(Task.FromResult(data));
            var channelUrl = ChannelUrlTools.CreateChannelUrl("https://youtube.com/channel/test");
            var a = await ChannelLiveFinder.FindLiveVidsAsync(serverMock.Object, channelUrl);
            Assert.AreEqual(new List<string> { "83xdTGcVYcg" }, a);
        }
    }
}
