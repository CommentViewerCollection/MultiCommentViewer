using CommentViewer.Plugin;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using YouTubeLiveSitePlugin;
using Moq.Protected;
using System.IO;
using System.Reflection;

namespace CommentGeneratorPluginTests
{
    [TestFixture]
    public class XmlWritingTests
    {
        string FilePath
        {
            get
            {
                var dir = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
                return Path.Combine(dir, "./comment_test.xml");
            }
        }
        [SetUp]
        public void Setup()
        {
        }
        class YtComment : IYouTubeLiveComment
        {
            public YouTubeLiveMessageType YouTubeLiveMessageType { get; } = YouTubeLiveMessageType.Comment;
            public string Id { get; set; }
            public string UserId { get; set; }
            public string PostTime { get; set; }
            public IMessageImage UserIcon { get; set; }
            public string Raw { get; set; }
            public SiteType SiteType { get; } = SiteType.YouTubeLive;
            public IEnumerable<IMessagePart> NameItems { get; set; }
            public IEnumerable<IMessagePart> CommentItems { get; set; }

            public event EventHandler<ValueChangedEventArgs> ValueChanged;
        }
        [Test]
        public void IYouTubeCommentがXMLに正常に書き出されるか()
        {
            var optionsMock = new Mock<Options>();
            optionsMock.Setup(o => o.IsEnabled).Returns(true);

            var messageMetadataMock = new Mock<IMessageMetadata>();
            var pluginMock = new Mock<CommentGeneratorPlugin>() { CallBase = true };
            pluginMock.Protected().Setup<string>("CommentXmlPath").Returns(FilePath);
            pluginMock.Protected().Setup<Options>("Options").Returns(optionsMock.Object);
            pluginMock.Protected().Setup<bool>("IsHcgSettingFileExists").Returns(true);
            pluginMock.Protected().Setup<DateTime>("GetCurrentDateTime").Returns(new DateTime(1970,1,1,0,0,0, DateTimeKind.Utc).ToLocalTime());

            var message = new YtComment()
            {
                NameItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText("name") },
                CommentItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText("comment") },
            };
            var messageMetadata = messageMetadataMock.Object;


            var plugin = pluginMock.Object;
            plugin.OnMessageReceived(message, messageMetadata);
            plugin.Write();

            if (!File.Exists(FilePath))
            {
                Assert.Fail();
            }

            string content;
            using (var sr = new System.IO.StreamReader(FilePath))
            {
                content = sr.ReadToEnd();
            }
            Assert.AreEqual("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<log>\r\n  <comment no=\"0\" time=\"0\" owner=\"0\" service=\"youtubelive\" handle=\"name\">comment</comment>\r\n</log>", content);
        }
        [TearDown]
        public void TearDown()
        {
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }
        }
    }
}
