using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using LineLiveSitePlugin;
using Moq;
using SitePlugin;
using Common;

namespace LineLiveSitePluginTests
{
    [TestFixture]
    class LineLiveCommentViewModelTests
    {
        [TestCase(true, false, Description = "NGユーザは非表示")]
        [TestCase(false, true, Description = "非NGユーザは表示")]
        public void VisibilityTest(bool isNgUser, bool isVisible)
        {
            var lineLiveUserMock = new Mock<LineLiveSitePlugin.ParseMessage.IUser>();
            lineLiveUserMock.Setup(u => u.Id).Returns(123);
            var messageMock = new Mock<LineLiveSitePlugin.ParseMessage.IMessageData>();
            var senderMock = new Mock<LineLiveSitePlugin.ParseMessage.IUser>();
            var optionsMock = new Mock<ICommentOptions>();

            var siteOptionsMock = new Mock<ILineLiveSiteOptions>();
            siteOptionsMock.Setup(s => s.IsAutoSetNickname).Returns(true);

            var cpMock = new Mock<ICommentProvider>();
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.IsNgUser).Returns(isNgUser);
            var message = messageMock.Object;
            var sender = senderMock.Object;
            var cvm = new LineLiveCommentViewModel(optionsMock.Object, siteOptionsMock.Object, message, sender, userMock.Object, cpMock.Object);
            Assert.AreEqual(isVisible, cvm.IsVisible);
        }
        [Test]
        public void 途中でNGユーザに指定された時に非表示になるか()
        {
            var userId = "abc";
            var lineLiveUserMock = new Mock<LineLiveSitePlugin.ParseMessage.IUser>();
            lineLiveUserMock.Setup(u => u.Id).Returns(123);
            var messageMock = new Mock<LineLiveSitePlugin.ParseMessage.IMessageData>();
            var senderMock = new Mock<LineLiveSitePlugin.ParseMessage.IUser>();
            var optionsMock = new Mock<ICommentOptions>();

            var siteOptionsMock = new Mock<ILineLiveSiteOptions>();
            siteOptionsMock.Setup(s => s.IsAutoSetNickname).Returns(true);

            var cpMock = new Mock<ICommentProvider>();
            var user = new UserTest(userId);
            user.IsNgUser = false;

            var message = messageMock.Object;
            var sender = senderMock.Object;
            var cvm = new LineLiveCommentViewModel(optionsMock.Object, siteOptionsMock.Object, message, sender, user, cpMock.Object);
            Assert.IsTrue(cvm.IsVisible);
            user.IsNgUser = true;
            Assert.IsFalse(cvm.IsVisible);
        }
        [Test]
        public void LineLive_コメントにニックネームが含まれていたらそれがニックネームにセットされるか()
        {
            var nick = "abc";
            var userMock = new Mock<IUser>();
            userMock.SetupGet(u => u.Nickname).Returns(nick);
            ICommentViewModel cvm = CreateCvm(userMock.Object, username: "", message: "@" + nick);
            Assert.AreEqual("abc", ((IMessageText)cvm.NameItems.ToList()[0]).Text);
        }
        [Test]
        public void LineLive_コメントにニックネームが含まれていたらそのユーザの他のCVMにも反映されるか()
        {
            var username = "name";
            var nick = "nick";
            var user = new UserTest(username);

            ICommentViewModel cvm1 = CreateCvm(user, username: username, message: "k");
            Assert.AreEqual(username, ((IMessageText)cvm1.NameItems.ToList()[0]).Text);

            ICommentViewModel cvm2 = CreateCvm(user, username: username, message: "@" + nick);
            Assert.AreEqual(nick, ((IMessageText)cvm2.NameItems.ToList()[0]).Text);
            Assert.AreEqual(nick, ((IMessageText)cvm1.NameItems.ToList()[0]).Text);
        }
        private LineLiveCommentViewModel CreateCvm(IUser user, string username, string message)
        {
            var lineLiveUserMock = new Mock<LineLiveSitePlugin.ParseMessage.IUser>();
            lineLiveUserMock.Setup(u => u.Id).Returns(123);
            var messageMock = new Mock<LineLiveSitePlugin.ParseMessage.IMessageData>();
            messageMock.Setup(m => m.Message).Returns(message);
            var senderMock = new Mock<LineLiveSitePlugin.ParseMessage.IUser>();
            senderMock.Setup(s => s.DisplayName).Returns(username);
            var optionsMock = new Mock<ICommentOptions>();

            var siteOptionsMock = new Mock<ILineLiveSiteOptions>();
            siteOptionsMock.Setup(s => s.IsAutoSetNickname).Returns(true);

            var cpMock = new Mock<ICommentProvider>();

            var messageData = messageMock.Object;            
            var sender = senderMock.Object;
            var cvm = new LineLiveCommentViewModel(optionsMock.Object, siteOptionsMock.Object, messageData, sender, user, cpMock.Object);
            return cvm;
        }
    }
}
