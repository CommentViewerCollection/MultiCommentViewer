using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhowatchSitePlugin;
using NUnit.Framework;
using Moq;
using SitePlugin;
using System.Windows.Media;
using Common;

namespace WhowatchSitePluginTests
{
    [TestFixture]
    class WhowatchCommentViewModelTests
    {
        [Test]
        public void Item2CommentViewModel()
        {
            var comment = new Comment
            {
                CommentType = "BY_PLAYITEM",
                Id = 416953550,
                ItemCount = 4,
                Message = "バーガーを4個プレゼントしました。",
                PlayItemPatternId = 5,
                User = new CommentUser
                {
                    AccountName = "ふ:motikun",
                    IconUrl = "https://img.whowatch.tv/user_files/2164770/profile_icon/1515164491739.jpeg",
                    Id = 2164770,
                    Name = "キエル",
                    UserPath = "w:motikun",
                    UserType = "VALID",
                }, 
            };
            var dict = new Dictionary<long, PlayItem>
            {
                {5, new PlayItem{ Id = 5, ImageUrl = "url", Name="test item", SmallImageUrl = "small url"} },
            };
            var backColor = Color.FromArgb(0xFF, 0xFF, 0, 0);
            var foreColor = Color.FromArgb(0xFF, 0, 0xFF, 0xFF);
            var optionsMock = new Mock<ICommentOptions>();
            var siteOptionsMock = new Mock<IWhowatchSiteOptions>();
            siteOptionsMock.SetupGet(k => k.ItemBackColor).Returns(backColor);
            siteOptionsMock.SetupGet(k => k.ItemForeColor).Returns(foreColor);
            var userMock = new Mock<IUser>();
            var cpMock = new Mock<ICommentProvider>();
            var cvm = new WhowatchCommentViewModel(comment, dict, optionsMock.Object, siteOptionsMock.Object, userMock.Object, cpMock.Object, false);
            Assert.AreEqual(CommentType.Item, cvm.CommentType);
            Assert.AreEqual(backColor, cvm.Background.Color);
            Assert.AreEqual(foreColor, cvm.Foreground.Color);
            Assert.AreEqual("test item×4", cvm.Info);
        }
        [TestCase(true, false, Description = "NGユーザは非表示")]
        [TestCase(false, true, Description = "非NGユーザは表示")]
        public void VisibilityTest(bool isNgUser, bool isVisible)
        {
            var options = new Mock<ICommentOptions>();
            var siteOptions = new Mock<IWhowatchSiteOptions>();
            siteOptions.Setup(o => o.NeedAutoSubNickname).Returns(true);
            var userMock = new Mock<IUser>();
            var commentUserMock = new Mock<CommentUser>();
            commentUserMock.Setup(c => c.Name).Returns("");
            var commentMock = new Mock<Comment>();
            commentMock.Setup(c => c.User).Returns(commentUserMock.Object);
            userMock.Setup(u => u.IsNgUser).Returns(isNgUser);
            var cp = new Mock<ICommentProvider>();
            var cvm = new WhowatchCommentViewModel(commentMock.Object, options.Object, siteOptions.Object, userMock.Object, cp.Object, true);
            Assert.AreEqual(isVisible, cvm.IsVisible);
        }
        [Test]
        public void 途中でNGユーザに指定された時に非表示になるか()
        {
            var userId = "abc";
            var options = new Mock<ICommentOptions>();
            var siteOptions = new Mock<IWhowatchSiteOptions>();
            siteOptions.Setup(o => o.NeedAutoSubNickname).Returns(true);
            var user = new UserTest(userId);
            user.IsNgUser = false;
            var commentUserMock = new Mock<CommentUser>();
            commentUserMock.Setup(c => c.Name).Returns("");
            var commentMock = new Mock<Comment>();
            commentMock.Setup(c => c.User).Returns(commentUserMock.Object);
            
            var cp = new Mock<ICommentProvider>();
            var cvm = new WhowatchCommentViewModel(commentMock.Object, options.Object, siteOptions.Object, user, cp.Object, true);
            Assert.IsTrue(cvm.IsVisible);
            user.IsNgUser = true;
            Assert.IsFalse(cvm.IsVisible);
        }
        [Test]
        public void Whowatch_コメントにニックネームが含まれていたらそれがニックネームにセットされるか()
        {
            var nick = "abc";
            var user = new UserTest("");
            ICommentViewModel cvm = CreateCvm(user, username: nick, message: "@" + nick);
            Assert.AreEqual("abc", ((IMessageText)cvm.NameItems.ToList()[0]).Text);
        }
        [Test]
        public void Whowatch_コメントにニックネームが含まれていたらそのユーザの他のCVMにも反映されるか()
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
        private WhowatchCommentViewModel CreateCvm(IUser user, string username, string message)
        {
            var options = new Mock<ICommentOptions>();
            var siteOptions = new Mock<IWhowatchSiteOptions>();
            siteOptions.Setup(o => o.NeedAutoSubNickname).Returns(true);
            var commentMock = new Mock<Comment>();
            var commentUserMock = new Mock<CommentUser>();
            commentUserMock.Setup(c => c.Name).Returns(username);
            commentMock.Setup(c => c.User).Returns(commentUserMock.Object);
            commentMock.Setup(c => c.Message).Returns(message);

            var cp = new Mock<ICommentProvider>();
            var cvm = new WhowatchCommentViewModel(commentMock.Object, options.Object, siteOptions.Object, user, cp.Object, true);
            return cvm;
        }
    }
}
