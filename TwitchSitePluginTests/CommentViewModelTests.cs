using TwitchSitePlugin;
using NUnit.Framework;
using SitePlugin;
using System.Linq;
using System.Collections.Generic;
using Moq;
using Common;
//using Common;
namespace TwitchSitePluginTests
{

    [TestFixture]
    public class CommentViewModelTests
    {
        [Test]
        public void Twitch_コメントにニックネームが含まれていたらそれがニックネームにセットされるか()
        {
            var nick = "abc";
            var user = new UserTest("");
            ICommentViewModel cvm = CreateCvm(user, username: "", message: "@" + nick);
            Assert.AreEqual("abc", ((IMessageText)cvm.NameItems.ToList()[0]).Text);
        }
        [Test]
        public void Twitch_コメントにニックネームが含まれていたらそのユーザの他のCVMにも反映されるか()
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
        private TwitchCommentViewModel CreateCvm(IUser user, string username, string message)
        {
            var options = new Mock<ICommentOptions>();
            var siteOptions = new TwitchSiteOptions
            {
                NeedAutoSubNickname = true
            };
            var commentDataMock = new Mock<ICommentData>();
            commentDataMock.SetupGet(c => c.Username).Returns(username);
            commentDataMock.SetupGet(c => c.Message).Returns(message);
            var userMock = new Mock<IUser>();
            var commentProvider = new Mock<ICommentProvider>();
            var cvm = new TwitchCommentViewModel(options.Object, siteOptions, commentDataMock.Object, false, commentProvider.Object, user);
            return cvm;
        }
        [TestCase(true, false, Description = "NGユーザは非表示")]
        [TestCase(false, true, Description = "非NGユーザは表示")]
        public void VisibilityTest(bool isNgUser, bool isVisible)
        {
            var options = new Mock<ICommentOptions>();
            var siteOptions = new Mock<ITwitchSiteOptions>();
            siteOptions.Setup(o => o.NeedAutoSubNickname).Returns(true);
            var userMock = new Mock<IUser>();
            var commentDataMock = new Mock<ICommentData>();
            userMock.Setup(u => u.IsNgUser).Returns(isNgUser);
            var commentProvider = new Mock<ICommentProvider>();
            var cvm = new TwitchCommentViewModel(options.Object, siteOptions.Object, commentDataMock.Object, false, commentProvider.Object, userMock.Object);
            Assert.AreEqual(isVisible, cvm.IsVisible);
        }
        [Test]
        public void 途中でNGユーザに指定された時に非表示になるか()
        {
            var userId = "abc";
            var options = new Mock<ICommentOptions>();
            var siteOptions = new Mock<ITwitchSiteOptions>();
            siteOptions.Setup(o => o.NeedAutoSubNickname).Returns(true);
            var user = new UserTest(userId)
            {
                IsNgUser = false
            };
            var commentDataMock = new Mock<ICommentData>();
            var commentProvider = new Mock<ICommentProvider>();
            
            var cvm = new TwitchCommentViewModel(options.Object, siteOptions.Object, commentDataMock.Object, false, commentProvider.Object, user);
            Assert.IsTrue(cvm.IsVisible);
            user.IsNgUser = true;
            Assert.IsFalse(cvm.IsVisible);
        }
    }
}
