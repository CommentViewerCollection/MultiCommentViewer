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

namespace LineLiveSitePluginTests
{
    [TestFixture]
    class MessageMetadataTests
    {
        /// <summary>
        /// OptionsのIsUserNameWrappingを変更した時にIsNameWrappingが連動して変更されるか
        /// </summary>
        [Test]
        public void IsNameWrappingTest()
        {
            var messageMock = new Mock<ILineLiveMessage>();
            var optionsMock = new Mock<ICommentOptions>();
            optionsMock.Setup(s => s.IsUserNameWrapping).Returns(false);

            var options = optionsMock.Object;
            var siteOptionsMock = new Mock<ILineLiveSiteOptions>();
            var userMock = new Mock<IUser>();
            var metadata = new MessageMetadata(messageMock.Object,options, siteOptionsMock.Object, userMock.Object,null,false);

            //変更前
            Assert.IsFalse(metadata.IsNameWrapping);

            //変更
            optionsMock.Setup(s => s.IsUserNameWrapping).Returns(true);

            //変更通知
            optionsMock.Raise(o => o.PropertyChanged += null, new PropertyChangedEventArgs(nameof(options.IsUserNameWrapping)));

            //テスト
            Assert.IsTrue(metadata.IsNameWrapping);
        }
    }
}
