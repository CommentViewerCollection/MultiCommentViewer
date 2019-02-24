using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Moq;
using NicoSitePlugin;
using NUnit.Framework;
using SitePlugin;

namespace NicoSitePluginTests
{
    [TestFixture]
    public class NicoCommentViewModelTests
    {
        [Test]
        public void コメントにニックネームが含まれていたらそれがニックネームにセットされるか()
        {
            var nick = "abc";
            var userMock = new Mock<IUser>();
            userMock.SetupGet(u => u.Nickname).Returns(nick);
            ICommentViewModel cvm = CreateCvm(userMock.Object, userid: "", message: "@" + nick);
            Assert.AreEqual("abc", ((IMessageText)cvm.NameItems.ToList()[0]).Text);
        }
        [Test]
        public void コメントにニックネームが含まれていたらそのユーザの他のCVMにも反映されるか()
        {
            var userid = "id";
            var nick = "nick";
            var user = new UserTest(userid);

            ICommentViewModel cvm1 = CreateCvm(user, userid: userid, message: "");
            Assert.AreEqual(userid, ((IMessageText)cvm1.NameItems.ToList()[0]).Text);
            ICommentViewModel cvm2 = CreateCvm(user, userid: userid, message: "@" + nick);
            
            Assert.AreEqual(nick, ((IMessageText)cvm2.NameItems.ToList()[0]).Text);
            Assert.AreEqual(nick, ((IMessageText)cvm1.NameItems.ToList()[0]).Text);
        }
        private NicoCommentViewModel2 CreateCvm(IUser user, string userid, string message)
        {
            var options = new Mock<ICommentOptions>();
            var nicoOptionsMock = new Mock<INicoSiteOptions>();
            nicoOptionsMock.Setup(s => s.IsAutoSetNickname).Returns(true);
            var chat = new Chat()
            {
                UserId = userid,
                Text = message,
            };
            var ms = new Mock<IMs>();
            var roomInfo = new Mock<RoomInfo>(ms.Object, "");
            var commentProvider = new Mock<ICommentProvider>();
            var siteOptions = nicoOptionsMock.Object;
            var cvm = new NicoCommentViewModel2(options.Object, siteOptions, chat, roomInfo.Object.RoomLabel, user, commentProvider.Object, false);
            return cvm;
        }

        [TestCase(true, true, false, true, Description="184表示時は184でも表示")]
        [TestCase(false, true, false, false, Description = "184非表示時は184は非表示")]
        [TestCase(false, false, false, true, Description = "184非表示時でも生IDは表示")]
        [TestCase(false, true, true, false, Description = "NGユーザは非表示")]
        [TestCase(true, false, true, false, Description = "NGユーザは非表示")]
        public void VisibilityTest(bool isShow184, bool is184, bool isNgUser, bool isVisible)
        {
            var options = new Mock<ICommentOptions>();
            var nicoOptions = new Mock<INicoSiteOptions>();
            nicoOptions.Setup(o => o.IsShow184).Returns(isShow184);
            nicoOptions.Setup(o => o.IsAutoSetNickname).Returns(true);
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.IsNgUser).Returns(isNgUser);
            var chat = new Chat()
            {
                UserId = is184 ? "abc" : "123",
                Text = "",
            };
            var ms = new Mock<IMs>();
            var roomInfo = new Mock<RoomInfo>(ms.Object, "");
            var commentProvider = new Mock<ICommentProvider>();
            var cvm = new NicoCommentViewModel2(options.Object, nicoOptions.Object, chat, roomInfo.Object.RoomLabel, userMock.Object, commentProvider.Object, false);
            Assert.AreEqual(isVisible, cvm.IsVisible);
        }
        [Test]
        public void 途中でNGユーザに指定された時に非表示になるか()
        {
            var userId = "abc";
            var options = new Mock<ICommentOptions>();
            var nicoOptions = new Mock<INicoSiteOptions>();
            nicoOptions.Setup(o => o.IsShow184).Returns(true);
            nicoOptions.Setup(o => o.IsAutoSetNickname).Returns(true);
            var user = new UserTest(userId)
            {
                IsNgUser = false
            };
            var chat = new Chat()
            {
                UserId = userId,
                Text = "",
            };
            var ms = new Mock<IMs>();
            var roomInfo = new Mock<RoomInfo>(ms.Object, "");
            var commentProvider = new Mock<ICommentProvider>();
            var cvm = new NicoCommentViewModel2(options.Object, nicoOptions.Object, chat, roomInfo.Object.RoomLabel, user, commentProvider.Object, false);            
            Assert.IsTrue(cvm.IsVisible);
            user.IsNgUser = true;
            Assert.IsFalse(cvm.IsVisible);
        }
    }
}
