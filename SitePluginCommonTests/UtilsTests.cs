using NUnit.Framework;
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
    }
}
