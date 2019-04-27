using Common;
using Moq;
using NUnit.Framework;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CommonTests
{
    [TestFixture]
    public class SQLiteUserStoreTests
    {
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
    }
}
