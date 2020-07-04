using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Moq;
using OpenrecSitePlugin;
using SitePlugin;
using Common;

namespace OpenrecSitePluginTests
{
    class CommentViewModelTests
    {
        public async Task Test()
        {
            var commentMock = new Mock<IOpenrecCommentData>();
            commentMock.Setup(s => s.Stamp).Returns(new Mock<IMessageImage>().Object);
            var optionsMock = new Mock<ICommentOptions>();

            var siteOptionsMock = new Mock<OpenrecSiteOptions>();
            siteOptionsMock.SetupGet(s => s.IsPlayStampMusic).Returns(true);
            siteOptionsMock.SetupGet(s => s.StampMusicFilePath).Returns("a");
            siteOptionsMock.SetupGet(s => s.IsPlayYellMusic).Returns(true);
            siteOptionsMock.SetupGet(s => s.YellMusicFilePath).Returns("a");


            var cpMock = new Mock<ICommentProvider>();
            var userMock = new Mock<IUser>();
            var cvm = new OpenrecCommentViewModel(commentMock.Object, optionsMock.Object, siteOptionsMock.Object, cpMock.Object, false, userMock.Object);
            await cvm.AfterCommentAdded();
        }
        [TestCase(true, false, Description = "NGユーザは非表示")]
        [TestCase(false, true, Description = "非NGユーザは表示")]
        public void VisibilityTest(bool isNgUser, bool isVisible)
        {
            var commentMock = new Mock<IOpenrecCommentData>();
            commentMock.Setup(s => s.Stamp).Returns(new Mock<IMessageImage>().Object);
            commentMock.Setup(s => s.NameIcons).Returns(new List<IMessagePart>());
            commentMock.Setup(s => s.Message).Returns("");
            var optionsMock = new Mock<ICommentOptions>();
            var siteOptionsMock = new Mock<IOpenrecSiteOptions>();
            siteOptionsMock.Setup(s => s.IsAutoSetNickname).Returns(true);
            var cpMock = new Mock<ICommentProvider>();
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.IsNgUser).Returns(isNgUser);
            var cvm = new OpenrecCommentViewModel(commentMock.Object, optionsMock.Object, siteOptionsMock.Object, cpMock.Object, false, userMock.Object);
            Assert.AreEqual(isVisible, cvm.IsVisible);
        }
        [Test]
        public void 途中でNGユーザに指定された時に非表示になるか()
        {
            var userId = "abc";
            var commentMock = new Mock<IOpenrecCommentData>();
            commentMock.Setup(s => s.Stamp).Returns(new Mock<IMessageImage>().Object);
            commentMock.Setup(s => s.NameIcons).Returns(new List<IMessagePart>());
            commentMock.Setup(s => s.Message).Returns("");
            var optionsMock = new Mock<ICommentOptions>();

            var siteOptionsMock = new Mock<IOpenrecSiteOptions>();
            siteOptionsMock.Setup(s => s.IsAutoSetNickname).Returns(true);
            var cpMock = new Mock<ICommentProvider>();
            var user = new UserTest(userId);
            user.IsNgUser = false;
            var cvm = new OpenrecCommentViewModel(commentMock.Object, optionsMock.Object, siteOptionsMock.Object, cpMock.Object, false, user);
            Assert.IsTrue(cvm.IsVisible);
            user.IsNgUser = true;
            Assert.IsFalse(cvm.IsVisible);
        }
        [Test]
        public void コメントにニックネームが含まれていたらそれがニックネームにセットされるか()
        {
            var nick = "abc";
            var user = new UserTest("");
            ICommentViewModel cvm = CreateCvm(user, username: "", message: "@" + nick);
            Assert.AreEqual("abc", ((IMessageText)cvm.NameItems.ToList()[0]).Text);
        }
        [Test]
        public void コメントにニックネームが含まれていたらそのユーザの他のCVMにも反映されるか()
        {
            var userId = "id";
            var username = "name";
            var nick = "nick";
            var user = new UserTest(userId);

            ICommentViewModel cvm1 = CreateCvm(user, username: username, message: "");
            Assert.AreEqual(username, ((IMessageText)cvm1.NameItems.ToList()[0]).Text);
            ICommentViewModel cvm2 = CreateCvm(user, username: username, message: "@" + nick);

            Assert.AreEqual(nick, ((IMessageText)cvm2.NameItems.ToList()[0]).Text);
            Assert.AreEqual(nick, ((IMessageText)cvm1.NameItems.ToList()[0]).Text);
        }
        private OpenrecCommentViewModel CreateCvm(IUser user, string username, string message)
        {
            var commentMock = new Mock<IOpenrecCommentData>();
            commentMock.Setup(s => s.Stamp).Returns(new Mock<IMessageImage>().Object);
            commentMock.Setup(s => s.NameIcons).Returns(new List<IMessagePart>());
            commentMock.Setup(s => s.Name).Returns(username);
            commentMock.Setup(s => s.Message).Returns(message);
            var optionsMock = new Mock<ICommentOptions>();

            var siteOptionsMock = new Mock<IOpenrecSiteOptions>();
            siteOptionsMock.Setup(o => o.IsAutoSetNickname).Returns(true);
            var cpMock = new Mock<ICommentProvider>();
            user.IsNgUser = false;
            var cvm = new OpenrecCommentViewModel(commentMock.Object, optionsMock.Object, siteOptionsMock.Object, cpMock.Object, false, user);
            return cvm;
        }
    }
}
