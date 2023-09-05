using Mcv.YouTubeLiveSitePlugin;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
namespace YouTubeLiveSitePluginTests
{
    [TestFixture]
    class VidResolverTests
    {
        [Test]
        public async Task ResolveVidFromWatchUrl()
        {
            var s = new VidResolver();
            var serverMock = new Mock<IYouTubeLiveServer>();
            var result1 = await s.GetVid(serverMock.Object, new WatchUrl("https://www.youtube.com/watch?v=Rs-WxTGgVus"));
            Assert.IsTrue(result1 is IVidResult);
            Assert.AreEqual("Rs-WxTGgVus", ((VidResult)result1).Vid);

            var result2 = await s.GetVid(serverMock.Object, new WatchUrl("https://www.youtube.com/watch?v=Rs-WxTGgVus&feature=test"));
            Assert.IsTrue(result2 is IVidResult);
            Assert.AreEqual("Rs-WxTGgVus", ((VidResult)result2).Vid);
        }
    }
}
