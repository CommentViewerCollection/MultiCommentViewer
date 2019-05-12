using Common;
using LineLiveSitePlugin;
using Moq;
using NUnit.Framework;
using SitePlugin;
using SitePluginCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineLiveSitePluginTests
{
    [TestFixture]
    class LineLiveSiteContextTests
    {
        private LineLiveSitePlugin.LineLiveSiteContext CreateSiteContext()
        {
            var optionsMock = new Mock<ICommentOptions>();
            var serverMock = new Mock<IDataServer>();
            var loggerMock = new Mock<ILogger>();
            var userStoreManagerMock = new Mock<IUserStoreManager>();
            var context = new LineLiveSitePlugin.LineLiveSiteContext(optionsMock.Object, serverMock.Object, loggerMock.Object, userStoreManagerMock.Object);
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
