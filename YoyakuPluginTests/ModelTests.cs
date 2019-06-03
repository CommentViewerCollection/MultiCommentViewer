using Common;
using Moq;
using NUnit.Framework;
using OpenrecYoyakuPlugin;
using Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoyakuPluginTests
{
    [TestFixture]
    class ModelTests
    {
        [Test]
        public void YoyakuTest()
        {
            IOptions options = new DynamicOptions();
            var hostMock = new Mock<IPluginHost>();
            var host = hostMock.Object;
            var user = new UserTest("abc");
            var model = new Model(options, host);

            Assert.AreEqual(0, model.RegisteredUsers.Count);

            //登録するテスト
            model.ReserveCommandPattern = "登録";
            model.SetComment("abc", "abc", "登録", user, Guid.NewGuid());
            Assert.AreEqual(1, model.RegisteredUsers.Count);
            Assert.AreEqual(new User(user) { Id = "abc" }, model.RegisteredUsers[0]);

            //削除するテスト
            model.DeleteCommandPattern = "消す";
            model.SetComment("abc", "abc", "消す", user, Guid.NewGuid());
            Assert.AreEqual(0, model.RegisteredUsers.Count);
        }
        [Test]
        public void RegexTestTest()
        {
            RegexTestInternal("a", "a", "True");
            RegexTestInternal("予約し(ます|たいです)", "予約します", "True");
            RegexTestInternal("予約し(ます|たいです)", "予約したいです", "True");
            RegexTestInternal("予約し(ます|たいです)", "予約し", "False");

        }
        private void RegexTestInternal(string pattern, string comment, string result)
        {
            IOptions options = new DynamicOptions();
            var hostMock = new Mock<IPluginHost>();
            var host = hostMock.Object;
            var user = new UserTest("abc");
            var model = new Model(options, host);
            var raised = false;
            model.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(model.TestResult))
                {
                    raised = true;
                }
            };
            model.TestPattern = pattern;
            model.TestComment = comment;
            Assert.AreEqual(result, model.TestResult);
            Assert.IsTrue(raised);
        }
    }
}
