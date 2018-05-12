using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwicasCommentViewer;
using Common;
using NUnit.Framework;
using SitePlugin;

namespace TwicasCommentViewerTests
{
    [TestFixture]
    public class SQLiteUserStoreTests
    {
        [Test]
        public void Test()
        {
            var name = new List<IMessagePart> { new MessageText("abc") };
            var user = new UserTest("123") { Name = name, IsNgUser = true, Nickname = "nick", BackColorArgb = "#FF000000", ForeColorArgb = "#FFFFFFFF" };
            var json = SQLiteUserStore.ToJson(user);
            var decodedUser = SQLiteUserStore.FromJson(json);
            Comp(user, decodedUser);
        }
        /// <summary>
        /// string値がnullの場合でもちゃんとSerialize,Deserializeできるか
        /// </summary>
        [Test]
        public void Test2()
        {
            var name = new List<IMessagePart> { new MessageText("abc") };
            var user = new UserTest("123") { Name = name, IsNgUser = true, Nickname = null, BackColorArgb = "#FF000000", ForeColorArgb = "#FFFFFFFF" };
            var json = SQLiteUserStore.ToJson(user);
            var decodedUser = SQLiteUserStore.FromJson(json);
            Comp(user, decodedUser);
        }
        private static void Comp(IUser user, IUser decodedUser)
        {
            Assert.AreEqual(user.UserId, decodedUser.UserId);
            CollectionAssert.AreEquivalent(user.Name, decodedUser.Name);
            Assert.AreEqual(user.Nickname, decodedUser.Nickname);
            Assert.AreEqual(user.BackColorArgb, decodedUser.BackColorArgb);
            Assert.AreEqual(user.ForeColorArgb, decodedUser.ForeColorArgb);
            Assert.AreEqual(user.IsNgUser, decodedUser.IsNgUser);
        }
    }
}
