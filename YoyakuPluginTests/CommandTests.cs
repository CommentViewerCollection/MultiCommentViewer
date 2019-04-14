using Common;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using OpenrecYoyakuPlugin;
using Plugin;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using YouTubeLiveSitePlugin;

namespace YoyakuPluginTests
{
    [TestFixture]
    public class Tests
    {
        [SetUp]
        public void Setup()
        {

        }
        private IYouTubeLiveComment CreateMessage(string name, string message, string userId)
        {
            var messageMock = new Mock<IYouTubeLiveComment>();
            messageMock.Setup(m => m.NameItems).Returns(new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(name) });
            messageMock.Setup(m => m.CommentItems).Returns(new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(message) });
            messageMock.Setup(m => m.UserId).Returns(userId);
            return messageMock.Object;
        }
        private IPluginHost CreatePluginHost(IOptions options)
        {
            var hostMock = new Mock<IPluginHost>();
            hostMock.Setup(h => h.LoadOptions(It.IsAny<string>())).Returns((Func<string, string>)(s =>
            {
                return options.Serialize();
            }));
            var host = hostMock.Object;
            return host;
        }
        [Test]
        public void もともとコテハンが付いていたユーザを登録した時に名前にコテハンは採用されているか()
        {
            var options = new DynamicOptions()
            {
                IsEnabled = true,
            };
            var host = CreatePluginHost(options);

            var vmMock = new Mock<SettingsViewModel>(host, options, Dispatcher.CurrentDispatcher);
            vmMock.Protected().Setup<DateTime>("GetCurrentDateTime").Returns(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToLocalTime());
            var vm = vmMock.Object;

            var pluginMock = new Mock<PluginBody>() { CallBase = true };
            pluginMock.Protected().Setup<SettingsViewModel>("CreateSettingsViewModel").Returns(vm);
            pluginMock.Protected().Setup<IOptions>("LoadOptions").Returns(options);

            var plugin = pluginMock.Object;
            plugin.Host = host;
            plugin.OnLoaded();

            var oldName = "name";
            var user = new UserTest("1")
            {
                 Nickname="nick",
            };
            var message = CreateMessage(oldName, "/yoyaku", "1");

            var messageMetadataMock = new Mock<IMessageMetadata>();
            messageMetadataMock.Setup(x => x.User).Returns(user);
            var messageMetadata = messageMetadataMock.Object;

            plugin.OnMessageReceived(message, messageMetadata);

            var ms = vm.RegisteredUsers.ToArray();
            var pluginUser = ms[0];

            Assert.AreEqual("nick", pluginUser.Name);
        }
        [Test]
        public void コテハンを変えた時に反映されるか()
        {
            var options = new DynamicOptions()
            {
                IsEnabled = true,
            };
            var host = CreatePluginHost(options);

            var vmMock = new Mock<SettingsViewModel>(host, options, Dispatcher.CurrentDispatcher);
            vmMock.Protected().Setup<DateTime>("GetCurrentDateTime").Returns(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToLocalTime());
            var vm = vmMock.Object;

            var pluginMock = new Mock<PluginBody>() { CallBase = true };
            pluginMock.Protected().Setup<SettingsViewModel>("CreateSettingsViewModel").Returns(vm);
            pluginMock.Protected().Setup<IOptions>("LoadOptions").Returns(options);

            var plugin = pluginMock.Object;
            plugin.Host = host;
            plugin.OnLoaded();

            var oldName = "name";
            var user = new UserTest("1");
            var message = CreateMessage(oldName, "/yoyaku", "1");
            var messageMetadataMock = new Mock<IMessageMetadata>();
            messageMetadataMock.Setup(x => x.User).Returns(user);
            var messageMetadata = messageMetadataMock.Object;

            plugin.OnMessageReceived(message, messageMetadata);

            var ms = vm.RegisteredUsers.ToArray();
            var pluginUser = ms[0];

            Assert.AreEqual(oldName, pluginUser.Name);

            //名前を変更
            var newName = "newname";
            user.Nickname= newName;

            Assert.AreEqual(newName, pluginUser.Name);
        }
        [TearDown]
        public void TearDown()
        {
        }
    }
    [TestFixture]
    public class コマンドテスト
    {
        PluginBody _plugin;
        IMessageMetadata _messageMetadata;
        SettingsViewModel _vm;
        [SetUp]
        public void Setup()
        {
            var options = new DynamicOptions()
            {
                IsEnabled = true,
            };
            var hostMock = new Mock<IPluginHost>();
            hostMock.Setup(h => h.LoadOptions(It.IsAny<string>())).Returns((Func<string, string>)(s =>
            {
                return options.Serialize();
            }));
            var host = hostMock.Object;

            var vmMock = new Mock<SettingsViewModel>(host, options, Dispatcher.CurrentDispatcher);
            vmMock.Protected().Setup<DateTime>("GetCurrentDateTime").Returns(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToLocalTime());
            var vm = vmMock.Object;
            _vm = vm;
            //vm.on
            var messageMetadataMock = new Mock<IMessageMetadata>();
            messageMetadataMock.Setup(m => m.User).Returns(new UserTest("1"));
            var pluginMock = new Mock<PluginBody>() { CallBase = true };
            pluginMock.Protected().Setup<SettingsViewModel>("CreateSettingsViewModel").Returns(vm);
            pluginMock.Protected().Setup<IOptions>("LoadOptions").Returns(options);


            var messageMetadata = messageMetadataMock.Object;
            _messageMetadata = messageMetadata;

            var plugin = pluginMock.Object;
            _plugin = plugin;
            plugin.Host = host;
            plugin.OnLoaded();
        }
        private void CheckResult(User[] expected)
        {
            var ms = _vm.RegisteredUsers.ToArray();
            Assert.AreEqual(expected, ms);
        }
        private void AddComment(string comment, string userId)
        {
            var messageMock = new Mock<IYouTubeLiveComment>();
            messageMock.Setup(m => m.NameItems).Returns(new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText("name") });
            messageMock.Setup(m => m.CommentItems).Returns(new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(comment) });
            messageMock.Setup(m => m.UserId).Returns(userId);
            var message = messageMock.Object;
            _plugin.OnMessageReceived(message, _messageMetadata);
        }
        [Test]
        public void 登録が可能か()
        {
            AddComment("/yoyaku", "1");
            var ms = _vm.RegisteredUsers.ToArray();
            CheckResult(new[]{
                new User(new UserTest("1")){  Id="1" },
                });
        }
        [Test]
        public void 複数人が登録可能か()
        {
            AddComment("/yoyaku", "1");
            AddComment("/yoyaku", "2");
            var ms = _vm.RegisteredUsers.ToArray();
            CheckResult(new[]{
                new User(new UserTest("1")){  Id="1" },
                new User(new UserTest("2")){  Id="2" },
                });
        }
        [Test]
        public void 登録済みの場合に重複登録されないか()
        {
            AddComment("/yoyaku", "1");
            AddComment("/yoyaku", "1");
            var ms = _vm.RegisteredUsers.ToArray();
            CheckResult(new[]{
                new User(new UserTest("1")){  Id="1" },
                });
        }
        [Test]
        public void 取り消しが正常に動作するか()
        {
            AddComment("/yoyaku", "1");
            AddComment("/yoyaku", "2");
            AddComment("/torikeshi", "1");
            var ms = _vm.RegisteredUsers.ToArray();
            CheckResult(new[]{
                new User(new UserTest("2")){  Id="2" },
                });
        }
        [TearDown]
        public void TearDown()
        {
        }
    }
}
