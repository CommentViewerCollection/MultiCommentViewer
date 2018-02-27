using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using TwitchSitePlugin;
using Moq;
using SitePlugin;
using Common;
namespace TwitchSitePluginTests
{
    [TestFixture]
    class TwitchSiteContextTests        
    {
        [Test]
        public void TwitchSite_IsValdUrlTest()
        {
            var optionsMock = new Mock<ICommentOptions>();
            var loggerMock = new Mock<ILogger>();
            var userStoreMock = new Mock<IUserStore>();
            var dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
            var site = new TwitchSiteContext(optionsMock.Object, loggerMock.Object, userStoreMock.Object, dispatcher);
            Assert.IsTrue(site.IsValidInput("https://www.twitch.tv/abc"));
            Assert.IsTrue(site.IsValidInput("https://www.twitch.tv/abc?"));
            Assert.IsTrue(site.IsValidInput("https://www.twitch.tv/abc/"));
            Assert.IsFalse(site.IsValidInput("https://www.twitch.tv/"));
            Assert.IsFalse(site.IsValidInput("https://www.twitch.tv/?"));
        }
    }
}
