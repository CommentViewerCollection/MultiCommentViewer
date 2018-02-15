using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SitePlugin;
using NicoSitePlugin.Test;
using NUnit.Framework;
using Moq;
namespace NicoSitePluginTests
{
    [TestFixture]
    class NicoSiteContextTests
    {
        [Test]
        public void NicoSite_IsValidUrlTest()
        {
            var optionsMock = new Mock<IOptions>();
            var site = new NicoSiteContext(optionsMock.Object);
            Assert.IsTrue(site.IsValidInput("lv123456"));
            Assert.IsTrue(site.IsValidInput("http://live.nicovideo.jp/watch/lv123456"));
            Assert.IsTrue(site.IsValidInput("http://live.nicovideo.jp/watch/lv123456?_topic=live_channel_program_onairs&ref=zeromypage_nicorepo"));
            Assert.IsFalse(site.IsValidInput("http://live.nicovideo.jp"));


        }
    }
}
