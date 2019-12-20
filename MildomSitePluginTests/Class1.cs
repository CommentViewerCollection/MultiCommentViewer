using MildomSitePlugin;
using Moq;
using NUnit.Framework;
using ryu_s.BrowserCookie;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace MildomSitePluginTests
{
    [TestFixture]
    public class MildomCommentProviderTests
    {
        [Test]
        public void GetUserInfoFromCookie_LoggedinUserTest()
        {
            var userValue = "%7B%22my_id%22%3A12345678%2C%22user_id%22%3A12345678%2C%22loginname%22%3A%22abc%22%2C%22status%22%3A10%2C%22avatar%22%3A%22http%3A%2F%2Fpbs.twimg.com%2Fprofile_images%2F1014534803364827139%2FvuSCBJ15.jpg%22%2C%22level%22%3A1%2C%22location%22%3A%22Japan%7CTokyo%22%2C%22country%22%3A%22Japan%22%2C%22finance_country%22%3A%22Japan%22%2C%22user_cluster%22%3A%22aws_japan%22%2C%22power_point%22%3A0%2C%22available_account%22%3A0%2C%22account%22%3A0%2C%22account_total%22%3A0%2C%22accessToken%22%3A%22e1b8213d-93cd-4b10-ae55-5374794a2ec5%22%7D";
            var browserProfileMock = new Mock<IBrowserProfile>();
            browserProfileMock.Setup(b => b.GetCookieCollection(It.IsAny<string>())).Returns(new List<Cookie>
            {
                new Cookie("user", userValue, "/", ""),
            });
            var browserProfile = browserProfileMock.Object;
            var userInfo = Tools.GetUserInfoFromCookie(browserProfile) as LoggedinUserInfo;
            Assert.IsNotNull(userInfo);
            Assert.AreEqual(new Guid("e1b8213d-93cd-4b10-ae55-5374794a2ec5"), userInfo.AccessToken);
            Assert.AreEqual("abc", userInfo.Loginname);
            Assert.AreEqual("12345678", userInfo.MyId);
            Assert.AreEqual("12345678", userInfo.UserId);
        }
        [Test]
        public void GetUserInfoFromCookie_AnonymousUserTest()
        {
            var userValue = "abc";
            var browserProfileMock = new Mock<IBrowserProfile>();
            browserProfileMock.Setup(b => b.GetCookieCollection(It.IsAny<string>())).Returns(new List<Cookie>
            {
                new Cookie("gid", userValue, "/", ""),
            });
            var browserProfile = browserProfileMock.Object;
            var userInfo = Tools.GetUserInfoFromCookie(browserProfile) as AnonymousUserInfo;
            Assert.IsNotNull(userInfo);
            Assert.AreEqual(userValue, userInfo.GuestId);
            Assert.IsTrue(Regex.IsMatch(userInfo.GuestName, "^guest\\d{6}$"));
        }
    }
}
