using Mcv.YouTubeLiveSitePlugin;
using NUnit.Framework;
using System.Net.Http;

namespace YouTubeLiveSitePluginTests
{
    [TestFixture]
    class YouTubeLiveServerTests
    {
        [Test]
        public void Test()
        {
            var server = new YouTubeLiveServer();
            Assert.ThrowsAsync<HttpRequestException>(async () =>
            {
                await server.GetAsync("http://int-main.net/api/404");
            });
        }
    }
}
