using Common;
using Moq;
using MultiCommentViewer;
using NicoSitePlugin;
using NUnit.Framework;
using Plugin;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiCommentViewerTests
{
    [TestFixture]
    class NicoCommentViewModelTests
    {
        [Test]
        public void ItemTest()
        {
            var itemMock = new Mock<INicoGift>();
            var item = itemMock.Object;
            var metadataMock = new Mock<IMessageMetadata>();
            var metadata = metadataMock.Object;
            var methodsMock = new Mock<IMessageMethods>();
            var methods = methodsMock.Object;
            var connectionStatusMock = new Mock<IConnectionStatus>();
            var connectionStatus = connectionStatusMock.Object;
            var optionsMock = new Mock<IOptions>();
            var options = optionsMock.Object;
            var cvm = new NicoCommentViewModel(item, metadata, methods, connectionStatus, options);

        }
        [Test]
        public void IsVisibleTest()
        {
            var messageMock = new Mock<INicoComment>();
            messageMock.Setup(m => m.Id).Returns("");
            var metadataMock = new Mock<IMessageMetadata>();
            var methodsMock = new Mock<IMessageMethods>();
            var connectionStatusMock = new Mock<IConnectionStatus>();
            var optionsMock = new Mock<IOptions>();

            var message = messageMock.Object;
            var metadata = metadataMock.Object;
            var methods = methodsMock.Object;
            var connectionStatus = connectionStatusMock.Object;
            var options = optionsMock.Object;

            var cvm = new NicoCommentViewModel(message, metadata, methods, connectionStatus, options);
            var isVisible = false;
            cvm.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(metadata.IsVisible):
                        isVisible = cvm.IsVisible;
                        break;
                }
            };
            metadataMock.Setup(u => u.IsVisible).Returns(true);
            metadataMock.Raise(c => c.PropertyChanged += null, new PropertyChangedEventArgs(nameof(metadata.IsVisible)));
            Assert.IsTrue(isVisible);
        }
        [Test]
        public void IsInvisibleTest()
        {
            var messageMock = new Mock<INicoComment>();
            messageMock.Setup(m => m.Id).Returns("");
            var metadataMock = new Mock<IMessageMetadata>();
            var methodsMock = new Mock<IMessageMethods>();
            var connectionStatusMock = new Mock<IConnectionStatus>();
            var optionsMock = new Mock<IOptions>();

            var message = messageMock.Object;
            var metadata = metadataMock.Object;
            var methods = methodsMock.Object;
            var connectionStatus = connectionStatusMock.Object;
            var options = optionsMock.Object;

            var cvm = new NicoCommentViewModel(message, metadata, methods, connectionStatus, options);
            var isVisible = true;
            cvm.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(metadata.IsVisible):
                        isVisible = cvm.IsVisible;
                        break;
                }
            };
            metadataMock.Setup(u => u.IsVisible).Returns(false);
            metadataMock.Raise(c => c.PropertyChanged += null, new PropertyChangedEventArgs(nameof(metadata.IsVisible)));
            Assert.IsFalse(isVisible);
        }
    }
}
