using LineLiveSitePlugin;
using MirrativSitePlugin;
using Moq;
using MultiCommentViewer;
using NicoSitePlugin;
using NUnit.Framework;
using OpenrecSitePlugin;
using Plugin;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using TwitchSitePlugin;
using WhowatchSitePlugin;
using YouTubeLiveSitePlugin;

namespace MultiCommentViewerTests
{
    [TestFixture]
    class CommentViewModelSiteColorTests
    {
        [Test]
        public void YouTubeLiveColorTest()
        {
            var messageMock = new Mock<IYouTubeLiveComment>();
            messageMock.Setup(m => m.Id).Returns("");

            Test1((meta, methods, name, options) =>
            {
                return new MultiCommentViewer.McvYouTubeLiveCommentViewModel(messageMock.Object, meta, methods, name, options);
            });
        }
        [Test]
        public void OpenrecColorTest()
        {
            var messageMock = new Mock<IOpenrecComment>();
            messageMock.Setup(m => m.Id).Returns("");

            Test1((meta, methods, name, options) =>
            {
                return new MultiCommentViewer.OpenrecCommentViewModel(messageMock.Object, meta, methods, name, options);
            });
        }
        [Test]
        public void TwitchColorTest()
        {
            var messageMock = new Mock<ITwitchComment>();
            messageMock.Setup(m => m.Id).Returns("");

            Test1((meta, methods, name, options) =>
            {
                return new MultiCommentViewer.TwitchCommentViewModel(messageMock.Object, meta, methods, name, options);
            });
        }
        [Test]
        public void NicoLiveColorTest()
        {
            var messageMock = new Mock<INicoComment>();
            messageMock.Setup(m => m.Id).Returns("");

            Test1((meta, methods, name, options) =>
            {
                return new MultiCommentViewer.NicoCommentViewModel(messageMock.Object, meta, methods, name, options);
            });
        }
        //[Test]
        //public void TwicasColorTest()
        //{
        //    var messageMock = new Mock<ITwicasComment>();
        //    messageMock.Setup(m => m.Id).Returns("");

        //    Test1((meta, methods, name, options) =>
        //    {
        //        return new MultiCommentViewer.TwicasCommentViewModel(messageMock.Object, meta, methods, name, options);
        //    });
        //}
        [Test]
        public void LineLiveColorTest()
        {
            var messageMock = new Mock<ILineLiveComment>();
            messageMock.Setup(m => m.Id).Returns("");

            Test1((meta, methods, name, options) =>
            {
                return new MultiCommentViewer.LineLiveCommentViewModel(messageMock.Object, meta, methods, name, options);
            });
        }
        [Test]
        public void WhowatchColorTest()
        {
            var messageMock = new Mock<IWhowatchComment>();
            messageMock.Setup(m => m.Id).Returns("");

            Test1((meta, methods, name, options) =>
            {
                return new MultiCommentViewer.McvWhowatchCommentViewModel(messageMock.Object, meta, methods, name, options);
            });
        }
        [Test]
        public void MirrativColorTest()
        {
            var messageMock = new Mock<IMirrativComment>();
            messageMock.Setup(m => m.Id).Returns("");

            Test1((meta, methods, name, options) =>
            {
                return new MultiCommentViewer.McvMirrativCommentViewModel(messageMock.Object, meta, methods, name, options);
            });
        }
        private void Test1(Func<IMessageMetadata, IMessageMethods, IConnectionStatus, IOptions, IMcvCommentViewModel> f)
        {
            var metadataMock = new Mock<IMessageMetadata>();
            var methodsMock = new Mock<IMessageMethods>();
            var optionsMock = new Mock<IOptions>();
            optionsMock.Setup(o => o.SiteConnectionColorType).Returns(SiteConnectionColorType.Site);
            optionsMock.Setup(o => o.IsEnabledSiteConnectionColor).Returns(true);

            optionsMock.Setup(o => o.YouTubeLiveBackColor).Returns(Colors.Red);
            optionsMock.Setup(o => o.YouTubeLiveForeColor).Returns(Colors.Blue);

            optionsMock.Setup(o => o.OpenrecBackColor).Returns(Colors.Red);
            optionsMock.Setup(o => o.OpenrecForeColor).Returns(Colors.Blue);
            optionsMock.Setup(o => o.TwitchBackColor).Returns(Colors.Red);
            optionsMock.Setup(o => o.TwitchForeColor).Returns(Colors.Blue);
            optionsMock.Setup(o => o.NicoLiveBackColor).Returns(Colors.Red);
            optionsMock.Setup(o => o.NicoLiveForeColor).Returns(Colors.Blue);
            optionsMock.Setup(o => o.TwicasBackColor).Returns(Colors.Red);
            optionsMock.Setup(o => o.TwicasForeColor).Returns(Colors.Blue);
            optionsMock.Setup(o => o.LineLiveBackColor).Returns(Colors.Red);
            optionsMock.Setup(o => o.LineLiveForeColor).Returns(Colors.Blue);
            optionsMock.Setup(o => o.WhowatchBackColor).Returns(Colors.Red);
            optionsMock.Setup(o => o.WhowatchForeColor).Returns(Colors.Blue);
            optionsMock.Setup(o => o.MirrativBackColor).Returns(Colors.Red);
            optionsMock.Setup(o => o.MirrativForeColor).Returns(Colors.Blue);
            var connectionStatusMock = new Mock<IConnectionStatus>();

            var metadata = metadataMock.Object;
            var methods = methodsMock.Object;
            var connectionStatus = connectionStatusMock.Object;
            
            var options = optionsMock.Object;
            var cvm = f(metadata, methods, connectionStatus, options);// new MultiCommentViewer.McvMirrativCommentViewModel(message, metadata, methods, connectionName, options);
            Assert.AreEqual(Colors.Red, cvm.Background.Color);
            Assert.AreEqual(Colors.Blue, cvm.Foreground.Color);
        }
    }
}
