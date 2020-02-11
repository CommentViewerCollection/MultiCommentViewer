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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using TwitchSitePlugin;
using WhowatchSitePlugin;
using YouTubeLiveSitePlugin;

namespace MultiCommentViewerTests
{
    /// <summary>
    /// Options.IsEnabledSiteConnectionColorがtrueの状態からfalseにした時に色の変更が反映されるか
    /// </summary>
    [TestFixture]
    class CommentViewModelConnectionColor戻すTest
    {
        [Test]
        public void YouTubeLiveColor戻すTest()
        {
            var messageMock = new Mock<IYouTubeLiveComment>();
            messageMock.Setup(m => m.Id).Returns("");

            Test1((meta, methods, name, options) =>
            {
                return new MultiCommentViewer.McvYouTubeLiveCommentViewModel(messageMock.Object, meta, methods, name, options);
            });
        }
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
            //messageMock.Setup(m => m.Id).Returns("");

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
            //messageMock.Setup(m => m.Id).Returns("");

            Test1((meta, methods, name, options) =>
            {
                return new MultiCommentViewer.McvMirrativCommentViewModel(messageMock.Object, meta, methods, name, options);
            });
        }
        private void Test1(Func<IMessageMetadata, IMessageMethods, IConnectionStatus, IOptions, IMcvCommentViewModel> f)
        {
            var metadataMock = new Mock<IMessageMetadata>();
            //戻したあとの色
            metadataMock.Setup(m => m.BackColor).Returns(Colors.Yellow);
            metadataMock.Setup(m => m.ForeColor).Returns(Colors.Pink);
            var methodsMock = new Mock<IMessageMethods>();
            var optionsMock = new Mock<IOptions>();
            optionsMock.Setup(o => o.SiteConnectionColorType).Returns(SiteConnectionColorType.Connection);
            optionsMock.Setup(o => o.IsEnabledSiteConnectionColor).Returns(true);
            var connectionStatusMock = new Mock<IConnectionStatus>();
            connectionStatusMock.Setup(c => c.BackColor).Returns(Colors.Red);
            connectionStatusMock.Setup(c => c.ForeColor).Returns(Colors.Blue);

            var metadata = metadataMock.Object;
            var methods = methodsMock.Object;
            var connectionStatus = connectionStatusMock.Object;
            var options = optionsMock.Object;
            var cvm = f(metadata, methods, connectionStatus, options);// new MultiCommentViewer.McvMirrativCommentViewModel(message, metadata, methods, connectionName, options);
            Assert.AreEqual(Colors.Red, cvm.Background.Color);
            Assert.AreEqual(Colors.Blue, cvm.Foreground.Color);

            var isRaisedBackground = false;
            var isRaisedForeground = false;
            cvm.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(cvm.Background):
                        isRaisedBackground = true;
                        break;
                    case nameof(cvm.Foreground):
                        isRaisedForeground = true;
                        break;
                }
            };
            optionsMock.Setup(o => o.IsEnabledSiteConnectionColor).Returns(false);
            optionsMock.Raise(c => c.PropertyChanged += null, new PropertyChangedEventArgs(nameof(options.IsEnabledSiteConnectionColor)));

            Assert.AreEqual(Colors.Yellow, cvm.Background.Color);
            Assert.AreEqual(Colors.Pink, cvm.Foreground.Color);
            Assert.IsTrue(isRaisedBackground);
            Assert.IsTrue(isRaisedForeground);

        }
    }
    [TestFixture]
    class CommentViewModelConnectionColorTest
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
            //messageMock.Setup(m => m.Id).Returns("");

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
            //messageMock.Setup(m => m.Id).Returns("");

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
            optionsMock.Setup(o => o.SiteConnectionColorType).Returns(SiteConnectionColorType.Connection);
            optionsMock.Setup(o => o.IsEnabledSiteConnectionColor).Returns(true);
            var connectionStatusMock = new Mock<IConnectionStatus>();
            connectionStatusMock.Setup(c => c.BackColor).Returns(Colors.Red);
            connectionStatusMock.Setup(c => c.ForeColor).Returns(Colors.Blue);

            var metadata = metadataMock.Object;
            var methods = methodsMock.Object;
            var connectionStatus = connectionStatusMock.Object;
            var options = optionsMock.Object;
            var cvm = f(metadata, methods, connectionStatus, options);// new MultiCommentViewer.McvMirrativCommentViewModel(message, metadata, methods, connectionName, options);
            Assert.AreEqual(Colors.Red, cvm.Background.Color);
            Assert.AreEqual(Colors.Blue, cvm.Foreground.Color);

            var isRaisedBackground = false;
            var isRaisedForeground = false;
            cvm.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(cvm.Background):
                        isRaisedBackground = true;
                        break;
                    case nameof(cvm.Foreground):
                        isRaisedForeground = true;
                        break;
                }
            };
            connectionStatusMock.Setup(c => c.BackColor).Returns(Colors.Blue);
            connectionStatusMock.Setup(c => c.ForeColor).Returns(Colors.Red);
            connectionStatusMock.Raise(c => c.PropertyChanged += null, new PropertyChangedEventArgs(nameof(connectionStatus.BackColor)));
            connectionStatusMock.Raise(c => c.PropertyChanged += null, new PropertyChangedEventArgs(nameof(connectionStatus.ForeColor)));
            Assert.AreEqual(Colors.Blue, cvm.Background.Color);
            Assert.AreEqual(Colors.Red, cvm.Foreground.Color);

            Assert.IsTrue(isRaisedBackground);
            Assert.IsTrue(isRaisedForeground);
        }
    }
}
