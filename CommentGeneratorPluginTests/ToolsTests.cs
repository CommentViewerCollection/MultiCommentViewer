using Moq;
using NUnit.Framework;
using OpenrecSitePlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YouTubeLiveSitePlugin;
using CommentViewer.Plugin;
using MildomSitePlugin;
using SitePlugin;

namespace CommentGeneratorPluginTests
{
    [TestFixture]
    class ToolsTests
    {
        public static object[][] Cases()
        {
            return new object[][]
            {
                new object[]{ new Mock<YouTubeLiveSitePlugin.IYouTubeLiveComment>().Object, "youtubelive" },
                new object[]{ new Mock<NicoSitePlugin.INicoComment>().Object, "nicolive" },
                new object[]{ new Mock<TwitchSitePlugin.ITwitchComment>().Object, "twitch" },
                new object[]{ new Mock<TwicasSitePlugin.ITwicasComment>().Object, "twicas" },
                new object[]{ new Mock<WhowatchSitePlugin.IWhowatchComment>().Object, "whowatch" },
                new object[]{ new Mock<OpenrecSitePlugin.IOpenrecComment>().Object, "openrec" },
                new object[]{ new Mock<MirrativSitePlugin.IMirrativComment>().Object, "mirrativ" },
                new object[]{ new Mock<MildomSitePlugin.IMildomComment>().Object, "mildom" },
            };
        }
        [TestCaseSourceAttribute(nameof(Cases))]
        public void GetSiteNameTest(ISiteMessage message, string expected)
        {
            Assert.AreEqual(expected, Tools.GetSiteName(message));
        }
    }
}