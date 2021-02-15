using Common;
using Moq;
using NUnit.Framework;
using SitePlugin;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CommonTests
{
    [TestFixture]
    public class SQLiteUserStoreTests
    {
        [Test]
        public void SaveTest()
        {
            var dbPath = GetTestDBPath();
            try
            {
                var loggerMock = new Mock<ILogger>();
                var logger = loggerMock.Object;
                var store1 = new Common.SQLiteUserStore(dbPath, logger);

                //ユーザ１，ユーザ２，ユーザ３を作成し、保存して終了する。
                store1.Init();
                {
                    var user1 = store1.GetUser("1");
                    user1.Nickname = "user1";
                    var user2 = store1.GetUser("2");
                    user2.Nickname = "user2";
                    var user3 = store1.GetUser("3");
                    user3.Nickname = "user3";
                }
                store1.Save();
                store1.ClearCache();

                //ユーザ１とユーザ３のみを読み込んでから保存する。
                store1.Init();
                {
                    var user1 = store1.GetUser("1");
                    var user3 = store1.GetUser("3");
                }
                store1.Save();
                store1.ClearCache();

                //ユーザ２を読み込んでコテハンが保存されているかチェックする
                store1.Init();
                {
                    var user2 = store1.GetUser("2");
                    Assert.AreEqual("user2", user2.Nickname);
                }
            }
            finally
            {
                if (File.Exists(dbPath))
                {
                    File.Delete(dbPath);
                }
            }
        }
        private string GetTestDBPath()
        {
            var dir = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = System.IO.Path.Combine(dir, "test.db");
            return path;
        }
        [Test]
        public void RestoreTest()
        {
            var dir = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = System.IO.Path.Combine(dir, "test.db");
            try
            {
                var loggerMock = new Mock<ILogger>();
                var logger = loggerMock.Object;
                var store = new Common.SQLiteUserStore(path, logger);
                store.Init();
                Assert.AreEqual(0, store.GetAllUsers().Count());
                var user1_1 = store.GetUser("1");
                user1_1.Name = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText("name") };
                user1_1.Nickname = "abc";
                user1_1.BackColorArgb = "#FFFF0000";
                user1_1.ForeColorArgb = "#FF0000FF";
                user1_1.IsNgUser = true;
                store.Save();
                store.ClearCache();

                var user1_2 = store.GetUser("1");
                Assert.AreEqual(new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText("name") }, user1_2.Name);
                Assert.AreEqual("abc", user1_2.Nickname);
                Assert.AreEqual("#FFFF0000", user1_2.BackColorArgb);
                Assert.AreEqual("#FF0000FF", user1_2.ForeColorArgb);
                Assert.IsTrue(user1_2.IsNgUser);
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }
        [Test]
        public void 同じユーザIDでUserAddedが重複して発生しないかテスト()
        {
            var loggerMock = new Mock<ILogger>();
            var logger = loggerMock.Object;
            var store = new Common.SQLiteUserStore(":memory:", logger);
            var dict = new Dictionary<string, IUser>();
            store.UserAdded += (s, e) =>
            {
                Assert.IsFalse(dict.ContainsKey(e.UserId));
            };
            store.Init();

            var list = new List<string>
            {
                "30662259",
                "30662259",
                "30662259",
                "30662259",
                "30662259",
                "30662259",
                "30662259",
                "30662259",
            };

            System.Threading.Tasks.Parallel.ForEach(list, s =>
            {
                var user = store.GetUser(s);
            });
        }
    }
}
