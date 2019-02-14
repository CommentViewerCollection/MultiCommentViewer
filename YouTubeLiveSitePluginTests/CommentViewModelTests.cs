using Common;
using Moq;
using NUnit.Framework;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YouTubeLiveSitePlugin;
using YouTubeLiveSitePlugin.Test2;

namespace YouTubeLiveSitePluginTests
{
    [TestFixture]
    class CommentViewModelTests
    {
        [TestCase(true, false, Description = "NGユーザは非表示")]
        [TestCase(false, true, Description = "非NGユーザは表示")]
        public void VisibilityTest(bool isNgUser, bool isVisible)
        {
            var options = new Mock<ICommentOptions>();
            var siteOptions = new Mock<IYouTubeLiveSiteOptions>();
            siteOptions.Setup(s => s.IsAutoSetNickname).Returns(true);
            var userMock = new Mock<IUser>();
            var commentDataMock = new Mock<CommentData>();
            userMock.Setup(u => u.IsNgUser).Returns(isNgUser);
            var cp = new Mock<ICommentProvider>();
            var cvm = new YouTubeLiveCommentViewModel(options.Object, siteOptions.Object, commentDataMock.Object, cp.Object, false, userMock.Object);
            Assert.AreEqual(isVisible, cvm.IsVisible);
        }
        [Test]
        public void 途中でNGユーザに指定された時に非表示になるか()
        {
            var userId = "abc";
            var options = new Mock<ICommentOptions>();
            var siteOptions = new Mock<IYouTubeLiveSiteOptions>();
            siteOptions.Setup(s => s.IsAutoSetNickname).Returns(true);
            var user = new UserTest(userId)
            {
                IsNgUser = false
            };
            var commentDataMock = new Mock<CommentData>();
            var cp = new Mock<ICommentProvider>();
            var cvm = new YouTubeLiveCommentViewModel(options.Object, siteOptions.Object, commentDataMock.Object, cp.Object, false, user);
            Assert.IsTrue(cvm.IsVisible);
            user.IsNgUser = true;
            Assert.IsFalse(cvm.IsVisible);
        }
        [Test]
        public void 既にニックネームが登録されているユーザがコメントをしたらニックネームが反映されるか()
        {
            var nick = "abc";
            var user = new UserTest("")
            {
                Nickname = nick
            };
            ICommentViewModel cvm = CreateCvm(user, username: nick, message: "xyz");
            Assert.AreEqual("abc", ((IMessageText)cvm.NameItems.ToList()[0]).Text);
        }
        [Test]
        public void YouTubeLive_コメントにニックネームが含まれていたらそれがニックネームにセットされるか()
        {
            var nick = "abc";
            var user = new UserTest("");
            ICommentViewModel cvm = CreateCvm(user, username: nick, message: "@" + nick);
            Assert.AreEqual("abc", ((IMessageText)cvm.NameItems.ToList()[0]).Text);
        }
        [Test]
        public void YouTubeLive_コメントにニックネームが含まれていたらそのユーザの他のCVMにも反映されるか()
        {
            var username = "name";
            var nick = "nick";
            var user = new UserTest("");
            ICommentViewModel cvm1 = CreateCvm(user, username: username, message: "");
            Assert.AreEqual(username, ((IMessageText)cvm1.NameItems.ToList()[0]).Text);

            ICommentViewModel cvm2 = CreateCvm(user, username: username, message: "@" + nick);

            Assert.AreEqual(nick, ((IMessageText)cvm2.NameItems.ToList()[0]).Text);
            Assert.AreEqual(nick, ((IMessageText)cvm1.NameItems.ToList()[0]).Text);
        }
        private YouTubeLiveCommentViewModel CreateCvm(IUser user, string username, string message)
        {
            var options = new Mock<ICommentOptions>();
            var siteOptions = new Mock<IYouTubeLiveSiteOptions>();
            siteOptions.Setup(s => s.IsAutoSetNickname).Returns(true);
            var commentData = new CommentData
            {
                MessageItems = new List<IMessagePart> { MessagePartFactory.CreateMessageText(message) },
                NameItems = new List<IMessagePart> { MessagePartFactory.CreateMessageText(username) },
            };
            var cp = new Mock<ICommentProvider>();
            var cvm = new YouTubeLiveCommentViewModel(options.Object, siteOptions.Object, commentData, cp.Object, false, user);
            return cvm;
        }
    }
}
