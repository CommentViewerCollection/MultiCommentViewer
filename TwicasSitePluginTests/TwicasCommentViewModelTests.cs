using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Moq;
using NUnit.Framework;
using SitePlugin;
using TwicasSitePlugin;

namespace TwicasSitePluginTests
{
    [TestFixture]
    public class TwicasCommentViewModelTests
    {
        [TestCase(true, false, Description = "NGユーザは非表示")]
        [TestCase(false, true, Description = "非NGユーザは表示")]
        public void VisibilityTest(bool isNgUser, bool isVisible)
        {
            var options = new Mock<ICommentOptions>();
            var siteOptions = new Mock<ITwicasSiteOptions>();
            siteOptions.Setup(s => s.IsAutoSetNickname).Returns(true);
            var userMock = new Mock<IUser>();
            var commentDataMock = new Mock<ICommentData>();
            userMock.Setup(u => u.IsNgUser).Returns(isNgUser);
            var commentProvider = new Mock<ICommentProvider>();
            var cvm = new TwicasCommentViewModel(options.Object,siteOptions.Object, commentDataMock.Object, userMock.Object, commentProvider.Object);
            Assert.AreEqual(isVisible, cvm.IsVisible);
        }
        [Test]
        public void 途中でNGユーザに指定された時に非表示になるか()
        {
            var userId = "abc";
            var options = new Mock<ICommentOptions>();
            var siteOptions = new Mock<ITwicasSiteOptions>();
            siteOptions.Setup(s => s.IsAutoSetNickname).Returns(true);
            var user = new UserTest(userId);
            user.IsNgUser = false;
            var commentDataMock = new Mock<ICommentData>();
            var commentProvider = new Mock<ICommentProvider>();
            var cvm = new TwicasCommentViewModel(options.Object,siteOptions.Object, commentDataMock.Object, user, commentProvider.Object);
            Assert.IsTrue(cvm.IsVisible);
            user.IsNgUser = true;
            Assert.IsFalse(cvm.IsVisible);
        }
        [Test]
        public void Twicas_コメントにニックネームが含まれていたらそれがニックネームにセットされるか()
        {
            var nick = "abc";
            var user = new UserTest("");
            ICommentViewModel cvm = CreateCvm(user, username: nick, message: "@" + nick);
            Assert.AreEqual("abc", ((IMessageText)cvm.NameItems.ToList()[0]).Text);
        }
        [Test]
        public void Twicas_コメントにニックネームが含まれていたらそのユーザの他のCVMにも反映されるか()
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
        private TwicasCommentViewModel CreateCvm(IUser user, string username, string message)
        {
            var options = new Mock<ICommentOptions>();
            var siteOptions = new Mock<ITwicasSiteOptions>();
            siteOptions.Setup(s => s.IsAutoSetNickname).Returns(true);
            var commentDataMock = new Mock<ICommentData>();
            commentDataMock.Setup(c => c.Message).Returns(new List<IMessagePart> { MessagePartFactory.CreateMessageText(message) });
            commentDataMock.Setup(c => c.Name).Returns(username);
            var commentProvider = new Mock<ICommentProvider>();
            var cvm = new TwicasCommentViewModel(options.Object, siteOptions.Object, commentDataMock.Object, user, commentProvider.Object);
            return cvm;
        }
    }
}
