using LineLiveSitePlugin;
using Moq;
using NUnit.Framework;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace LineLiveSitePluginTests
{
    [TestFixture]
    class MessageMetadataTests
    {
        /// <summary>
        /// 初コメ時に指定したフォントと色が反映されるか
        /// </summary>
        [Test]
        public void FirstCommentColorTest()
        {
            var userMock = new Mock<IUser>();
            var messageMock = new Mock<ILineLiveComment>();
            var optionsMock = new Mock<ICommentOptions>();
            optionsMock.Setup(o => o.FirstCommentBackColor).Returns(Colors.Blue);
            optionsMock.Setup(o => o.FirstCommentForeColor).Returns(Colors.Red);
            optionsMock.Setup(o => o.FirstCommentFontFamily).Returns(new FontFamily("Helvetica Neue"));
            optionsMock.Setup(o => o.FirstCommentFontWeight).Returns(FontWeights.ExtraLight);
            optionsMock.Setup(o => o.FirstCommentFontStyle).Returns(FontStyles.Italic);
            var siteOptionsMock = new Mock<ILineLiveSiteOptions>();
            var message = messageMock.Object;
            var options = optionsMock.Object;
            var siteOptions = siteOptionsMock.Object;
            var user = userMock.Object;

            var metadata = new MessageMetadata(message, options, siteOptions, user, null, true);
            Assert.AreEqual(Colors.Blue, metadata.BackColor);
            Assert.AreEqual(Colors.Red, metadata.ForeColor);
            Assert.AreEqual(new FontFamily("Helvetica Neue"), metadata.FontFamily);
            Assert.AreEqual(FontWeights.ExtraLight, metadata.FontWeight);
            Assert.AreEqual(FontStyles.Italic, metadata.FontStyle);
        }
        /// <summary>
        /// ユーザの背景色や文字色が指定されていたらそれが反映されるか
        /// </summary>
        [Test]
        public void UserColorTest()
        {
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.BackColorArgb).Returns("#FFFF0000");
            userMock.Setup(u => u.ForeColorArgb).Returns("#FF0000FF");
            var messageMock = new Mock<ILineLiveComment>();
            var optionsMock = new Mock<ICommentOptions>();
            var siteOptionsMock = new Mock<ILineLiveSiteOptions>();
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
            var messageMock = new Mock<ILineLiveComment>();
            var optionsMock = new Mock<ICommentOptions>();
            var siteOptionsMock = new Mock<ILineLiveSiteOptions>();
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
