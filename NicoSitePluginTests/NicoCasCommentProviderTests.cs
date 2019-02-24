using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NicoSitePlugin;
using Moq;
using SitePlugin;

namespace NicoSitePluginTests
{
    [TestFixture]
    class NicoCasCommentProviderTests
    {
        [Test]
        public void IsValidInputTest()
        {
            //var optionsMock = new Mock<ICommentOptions>();
            //var b = new Mock<INicoSiteOptions>();
            //var c = new Mock<IUserStore>();
            //var d = new Mock<IDataSource>();
            //var e = new Mock<Common.ILogger>();
            //var f = new Mock<ICommentProvider>();
            //var a = new NicoSitePlugin.NicoCasCommentProvider(optionsMock.Object, b.Object, c.Object, d.Object, e.Object, f.Object);

            var (isValid, userId, liveId) = NicoCasCommentProvider.IsValidInputWithUserId("https://cas.nicovideo.jp/user/38655/lv316253164");
            Assert.IsTrue(isValid);
            Assert.AreEqual("38655", userId);
            Assert.AreEqual("lv316253164", liveId);
        }
    }
}
