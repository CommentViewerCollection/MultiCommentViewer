using LineLiveSitePlugin;
using Mcv.PluginV2;
using Moq;
using NUnit.Framework;

namespace LineLiveSitePluginTests
{
    [TestFixture]
    class LineLiveSiteContextTests
    {
        private LineLiveSitePlugin.LineLiveSiteContext CreateSiteContext()
        {
            var serverMock = new Mock<IDataServer>();
            var loggerMock = new Mock<ILogger>();
            var context = new LineLiveSitePlugin.LineLiveSiteContext(serverMock.Object, loggerMock.Object);
            return context;
        }
        [Test]
        public void IsValidInput_null_Test()
        {
            var context = CreateSiteContext();
            Assert.IsFalse(context.IsValidInput(null));
        }
        [Test]
        public void IsValidInput_ChannelUrl_Test()
        {
            var context = CreateSiteContext();
            Assert.IsTrue(context.IsValidInput("https://live.line.me/channels/14578&test"));
        }
        [Test]
        public void IsValidInput_LiveUrl_Test()
        {
            var context = CreateSiteContext();
            Assert.IsTrue(context.IsValidInput("https://live.line.me/channels/14578/broadcast/8840007&test"));
        }
        [Test]
        public void IsValidInput_Invalid_Input_Test()
        {
            var context = CreateSiteContext();
            Assert.IsFalse(context.IsValidInput("abc"));
        }
    }
}
