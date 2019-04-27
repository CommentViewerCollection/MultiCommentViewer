using Moq;
using NUnit.Framework;
using PeriscopeSitePlugin;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeriscopeSitePluginTests
{
    [TestFixture]
    public class ApiTests
    {
        [Test]
        public async Task GetAccessVideoPublicAsyncTest()
        {
            var data = TestHelper.GetSampleData("AccessVideoPublic.txt");
            var serverMock = new Mock<IDataServer>();
            serverMock.Setup(s => s.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(data));
            var (a,b) = await Api.GetAccessVideoPublicAsync(serverMock.Object, "abc");
            Assert.IsTrue(!string.IsNullOrEmpty(a.ChatToken));

        }
    }
}
