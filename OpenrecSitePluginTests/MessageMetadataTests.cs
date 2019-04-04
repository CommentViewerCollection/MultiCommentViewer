using Moq;
using OpenrecSitePlugin;
using NUnit.Framework;
using SitePlugin;
using System.Windows.Media;
using System.ComponentModel;

namespace OpenrecSitePluginTests
{
    [TestFixture]
    class MessageMetadataTests
    {
        /// <summary>
        /// ユーザの背景色や文字色が指定されていたらそれが反映されるか
        /// </summary>
        [Test]
        public void UserColorTest()
        {
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.BackColorArgb).Returns("#FFFF0000");
            userMock.Setup(u => u.ForeColorArgb).Returns("#FF0000FF");
            var messageMock = new Mock<IOpenrecComment>();
            var optionsMock = new Mock<ICommentOptions>();
            var siteOptionsMock = new Mock<IOpenrecSiteOptions>();
            var message = messageMock.Object;
            var options = optionsMock.Object;
            var siteOptions = siteOptionsMock.Object;
            var user = userMock.Object;

            var metadata = new MessageMetadata(message, options, siteOptions, user, null, false);
            Assert.AreEqual(Colors.Red, metadata.BackColor);
            Assert.AreEqual(Colors.Blue, metadata.ForeColor);
        }
        /// <summary>
        /// ユーザの背景色文字色を変更したらMessageMetadataのProperptyChangedが発火するか
        /// </summary>
        [Test]
        public void UserColorChangedRaisedTest()
        {
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.BackColorArgb).Returns("#FFFF0000");
            userMock.Setup(u => u.ForeColorArgb).Returns("#FF0000FF");
            var messageMock = new Mock<IOpenrecComment>();
            var optionsMock = new Mock<ICommentOptions>();
            var siteOptionsMock = new Mock<IOpenrecSiteOptions>();
            var message = messageMock.Object;
            var options = optionsMock.Object;
            var siteOptions = siteOptionsMock.Object;
            var user = userMock.Object;

            var metadata = new MessageMetadata(message, options, siteOptions, user, null, false);
            var backColorRaised = false;
            var foreColorRaised = false;
            metadata.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(metadata.BackColor):
                        backColorRaised = true;
                        break;
                    case nameof(metadata.ForeColor):
                        foreColorRaised = true;
                        break;
                }
            };
            userMock.Raise(c => c.PropertyChanged += null, new PropertyChangedEventArgs(nameof(user.BackColorArgb)));
            userMock.Raise(c => c.PropertyChanged += null, new PropertyChangedEventArgs(nameof(user.ForeColorArgb)));
            Assert.IsTrue(backColorRaised);
            Assert.IsTrue(foreColorRaised);
        }
    }
}
