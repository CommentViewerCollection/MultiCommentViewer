using Common;
using Moq;
using MultiCommentViewer;
using NicoSitePlugin;
using NUnit.Framework;
using Plugin;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiCommentViewerTests
{
    [TestFixture]
    class NicoCommentViewModelTests
    {
        [Test]
        public async Task Test()
        {
            var userId = "1";
            var user = new UserTest(userId)
            {
                 Name= new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText("name1") },
            };
            var serverMock = new Mock<IDataSource>();
            var server = serverMock.Object;
            var loggerMock = new Mock<ILogger>();
            var logger = loggerMock.Object;
            var optionsMock = new Mock<IOptions>();
            var options = optionsMock.Object;

            var metadataMock = new Mock<IMessageMetadata>();
            metadataMock.Setup(m => m.User).Returns(user);
            var methodsMock = new Mock<IMessageMethods>();

            var metadata = metadataMock.Object;
            var methods = methodsMock.Object;
            var connectionName = new ConnectionName();
            var connectionStatus = new Mock<IConnectionStatus>().Object;

            var chat1 = new Chat("<chat thread=\"1645724171\" no=\"4\" vpos=\"180000\" date=\"1550890471\" date_usec=\"549074\" mail=\"184\" user_id=\"G-lRat9seQmpK-gcgcQXSFxr14c\" premium=\"1\" anonymity=\"1\" locale=\"ja-jp\">message1</chat>");
            var comment1 = await Tools.CreateNicoCommentAsync(chat1, "", user, server, true, "", logger);
            var cvm1 = new NicoCommentViewModel(comment1, metadata, methods, connectionStatus, options);
            Assert.IsNull(cvm1.NameItems);

            var chat2 = new Chat("<chat thread=\"1645724171\" no=\"4\" vpos=\"180000\" date=\"1550890471\" date_usec=\"549074\" mail=\"184\" user_id=\"G-lRat9seQmpK-gcgcQXSFxr14c\" premium=\"1\" anonymity=\"1\" locale=\"ja-jp\">message2@newnick</chat>");
            var comment2 = await Tools.CreateNicoCommentAsync(chat2, "", user, server, true, "", logger);
            var cvm2 = new NicoCommentViewModel(comment2, metadata, methods, connectionStatus, options);
            Assert.AreEqual(new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText("newnick") }, cvm1.NameItems);
            Assert.AreEqual(new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText("newnick") }, cvm2.NameItems);
        }
    }
}
