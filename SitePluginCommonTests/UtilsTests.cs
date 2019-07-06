using Moq;
using NUnit.Framework;
using SitePlugin;
using SitePluginCommon;

namespace SitePluginCommonTests
{
    [TestFixture]
    public class UtilsTests
    {
        [Test]
        public void ExtractNicknameTest()
        {
            Assert.IsTrue(string.IsNullOrEmpty(Utils.ExtractNickname("@123")));
            Assert.IsTrue(string.IsNullOrEmpty(Utils.ExtractNickname("@１２３")));
            Assert.IsTrue(string.IsNullOrEmpty(Utils.ExtractNickname("test")));
            Assert.AreEqual("test", Utils.ExtractNickname("@test"));
            Assert.AreEqual("test", Utils.ExtractNickname("＠test"));
            Assert.AreEqual("test", Utils.ExtractNickname("@abc@test"));
            Assert.AreEqual("test", Utils.ExtractNickname("@@test"));
        }
        [TestCase("@", "abc@def@test", "test")]
        [TestCase("@|＠", "abc@def＠test", "test")]
        [TestCase("$", "abc\\@test", null)]
        [TestCase("$", "abc\\$test", "test")]
        [TestCase("\\", "abc\\test", "test")]
        [TestCase("\"", "abc\"test", "test")]
        [TestCase("(", "abc(test", "test")]
        [TestCase(")", "abc)test", "test")]
        [TestCase("[", "abc[test", "test")]
        [TestCase("]", "abc]test", "test")]
        [TestCase(":", "abc:test", "test")]
        [TestCase("\\|?", "abc\\test", "test")]
        [TestCase("\\|?", "abc?test", "test")]
        [TestCase("|", "abc|test", "test")]
        [TestCase("| ", "abc| test", "test")]
        [TestCase("| ", "abc|test", null)]
        [TestCase("$|", "abc$test", null, Description = "\"|\"の前後にあいう")]
        public void SetNicknameTest(string matchStr, string text, string expected)
        {
            var userMock = new Mock<IUser>();
            Utils.SetNickname(text, userMock.Object, matchStr);
            if (string.IsNullOrEmpty(expected))
            {
                userMock.VerifySet(u => u.Nickname = expected, Times.Never);
            }
            else
            {
                userMock.VerifySet(u => u.Nickname = expected);
            }

        }
    }
}

