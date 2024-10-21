using Moq;
using NUnit.Framework;
using PluginCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginCommonTests
{
    public class Class1
    {
        [Test]
        public void GetDataTest()
        {
            const string nameExpected = "name";
            const string commentExpected = "comment";
            {
                var m = new Mock<YouTubeLiveSitePlugin.IYouTubeLiveComment>();

                m.Setup(x => x.NameItems).Returns(Common.MessagePartFactory.CreateMessageItems(nameExpected));
                m.Setup(x => x.CommentItems).Returns(Common.MessagePartFactory.CreateMessageItems(commentExpected));
                var obj = m.Object;

                var (nameActual, commentActual) = Tools.GetData(obj);
                Assert.AreEqual(nameExpected, nameActual);
                Assert.AreEqual(commentExpected, commentActual);
            }
            {
                var m = new Mock<YouTubeLiveSitePlugin.IYouTubeLiveSuperchat>();

                m.Setup(x => x.NameItems).Returns(Common.MessagePartFactory.CreateMessageItems(nameExpected));
                m.Setup(x => x.CommentItems).Returns(Common.MessagePartFactory.CreateMessageItems(commentExpected));
                var obj = m.Object;

                var (nameActual, commentActual) = Tools.GetData(obj);
                Assert.AreEqual(nameExpected, nameActual);
                Assert.AreEqual(commentExpected, commentActual);
            }
            {
                var m = new Mock<NicoSitePlugin.INicoComment>();

                m.Setup(x => x.UserName).Returns(nameExpected);
                m.Setup(x => x.Text).Returns(commentExpected);
                var obj = m.Object;

                var (nameActual, commentActual) = Tools.GetData(obj);
                Assert.AreEqual(nameExpected, nameActual);
                Assert.AreEqual(commentExpected, commentActual);
            }
            {
                var m = new Mock<NicoSitePlugin.INicoGift>();

                m.Setup(x => x.Text).Returns(commentExpected);
                var obj = m.Object;

                var (nameActual, commentActual) = Tools.GetData(obj);
                Assert.AreEqual(commentExpected, commentActual);
            }
            {
                var m = new Mock<TwicasSitePlugin.ITwicasComment>();

                m.Setup(x => x.UserName).Returns(nameExpected);
                m.Setup(x => x.CommentItems).Returns(Common.MessagePartFactory.CreateMessageItems(commentExpected));
                var obj = m.Object;

                var (nameActual, commentActual) = Tools.GetData(obj);
                Assert.AreEqual(nameExpected, nameActual);
                Assert.AreEqual(commentExpected, commentActual);
            }
            {
                var m = new Mock<TwicasSitePlugin.ITwicasKiitos>();

                m.Setup(x => x.UserName).Returns(nameExpected);
                m.Setup(x => x.CommentItems).Returns(Common.MessagePartFactory.CreateMessageItems(commentExpected));
                var obj = m.Object;

                var (nameActual, commentActual) = Tools.GetData(obj);
                Assert.AreEqual(nameExpected, nameActual);
                Assert.AreEqual(commentExpected, commentActual);
            }
            {
                var m = new Mock<TwicasSitePlugin.ITwicasItem>();

                m.Setup(x => x.UserName).Returns(nameExpected);
                m.Setup(x => x.CommentItems).Returns(Common.MessagePartFactory.CreateMessageItems(commentExpected));
                var obj = m.Object;

                var (nameActual, commentActual) = Tools.GetData(obj);
                Assert.AreEqual(nameExpected, nameActual);
                Assert.AreEqual(commentExpected, commentActual);
            }
            {
                var m = new Mock<WhowatchSitePlugin.IWhowatchComment>();

                m.Setup(x => x.UserName).Returns(nameExpected);
                m.Setup(x => x.Comment).Returns(commentExpected);
                var obj = m.Object;

                var (nameActual, commentActual) = Tools.GetData(obj);
                Assert.AreEqual(nameExpected, nameActual);
                Assert.AreEqual(commentExpected, commentActual);
            }
            {
                var m = new Mock<WhowatchSitePlugin.IWhowatchItem>();

                m.Setup(x => x.UserName).Returns(nameExpected);
                m.Setup(x => x.Comment).Returns(commentExpected);
                var obj = m.Object;

                var (nameActual, commentActual) = Tools.GetData(obj);
                Assert.AreEqual(nameExpected, nameActual);
                Assert.AreEqual(commentExpected, commentActual);
            }
            {
                var m = new Mock<OpenrecSitePlugin.IOpenrecComment>();

                m.Setup(x => x.NameItems).Returns(Common.MessagePartFactory.CreateMessageItems(nameExpected));
                m.Setup(x => x.MessageItems).Returns(Common.MessagePartFactory.CreateMessageItems(commentExpected));
                var obj = m.Object;


                var (nameActual, commentActual) = Tools.GetData(obj);
                Assert.AreEqual(nameExpected, nameActual);
                Assert.AreEqual(commentExpected, commentActual);
            }
            {
                var m = new Mock<BLiveSitePlugin.IBLiveComment>();

                m.Setup(x => x.NameItems).Returns(Common.MessagePartFactory.CreateMessageItems(nameExpected));
                m.Setup(x => x.MessageItems).Returns(Common.MessagePartFactory.CreateMessageItems(commentExpected));
                var obj = m.Object;


                var (nameActual, commentActual) = Tools.GetData(obj);
                Assert.AreEqual(nameExpected, nameActual);
                Assert.AreEqual(commentExpected, commentActual);
            }
            {
                var m = new Mock<MixchSitePlugin.IMixchMessage>();

                m.Setup(x => x.NameItems).Returns(Common.MessagePartFactory.CreateMessageItems(nameExpected));
                m.Setup(x => x.MessageItems).Returns(Common.MessagePartFactory.CreateMessageItems(commentExpected));
                var obj = m.Object;


                var (nameActual, commentActual) = Tools.GetData(obj);
                Assert.AreEqual(nameExpected, nameActual);
                Assert.AreEqual(commentExpected, commentActual);
            }
            {
                var m = new Mock<MirrativSitePlugin.IMirrativComment>();

                m.Setup(x => x.UserName).Returns(nameExpected);
                m.Setup(x => x.Text).Returns(commentExpected);
                var obj = m.Object;

                var (nameActual, commentActual) = Tools.GetData(obj);
                Assert.AreEqual(nameExpected, nameActual);
                Assert.AreEqual(commentExpected, commentActual);
            }
            {
                var m = new Mock<LineLiveSitePlugin.ILineLiveComment>();

                m.Setup(x => x.DisplayName).Returns(nameExpected);
                m.Setup(x => x.Text).Returns(commentExpected);
                var obj = m.Object;

                var (nameActual, commentActual) = Tools.GetData(obj);
                Assert.AreEqual(nameExpected, nameActual);
                Assert.AreEqual(commentExpected, commentActual);
            }
            {
                var m = new Mock<PeriscopeSitePlugin.IPeriscopeComment>();

                m.Setup(x => x.DisplayName).Returns(nameExpected);
                m.Setup(x => x.Text).Returns(commentExpected);
                var obj = m.Object;

                var (nameActual, commentActual) = Tools.GetData(obj);
                Assert.AreEqual(nameExpected, nameActual);
                Assert.AreEqual(commentExpected, commentActual);
            }
            {
                var m = new Mock<MildomSitePlugin.IMildomComment>();
                m.Setup(x => x.UserName).Returns(nameExpected);
                m.Setup(x => x.CommentItems).Returns(Common.MessagePartFactory.CreateMessageItems(commentExpected));
                var obj = m.Object;

                var (nameActual, commentActual) = Tools.GetData(obj);
                Assert.AreEqual(nameExpected, nameActual);
                Assert.AreEqual(commentExpected, commentActual);
            }
        }
    }
}
