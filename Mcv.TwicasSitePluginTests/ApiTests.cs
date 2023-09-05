using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TwicasSitePlugin;
using YouTubeLiveSitePluginTests;

namespace TwicasSitePluginTests
{
    [TestFixture]
    class ApiTests
    {
        [Test]
        public async Task Test()
        {
            var data = TestHelper.GetSampleData("ListAll.txt");
            var serverMock = new Mock<IDataServer>();
            serverMock.Setup(s => s.GetAsync(It.IsAny<string>(), It.IsAny<CookieContainer>())).Returns(Task.FromResult(data));
            var server = serverMock.Object;
            var (comments, raw) = await API.GetListAll(server, "", 0, 0, 0, 0, new System.Net.CookieContainer());
            Assert.AreEqual(20, comments.Length);
            var c = comments[0];
            Assert.AreEqual("comment", c.Type);
            Assert.AreEqual(1583166022000, c.CreatedAt);
            Assert.IsNull(c.HtmlMessage);
            Assert.AreEqual(17919844422, c.Id);
            Assert.AreEqual("次いでによっちゃんも残して死なないで", c.Message);
            Assert.AreEqual("エコハ@🍃", c.Author.Name);
        }
    }
}
