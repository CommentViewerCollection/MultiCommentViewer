using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YouTubeLiveSitePlugin.Test2;

namespace YouTubeLiveSitePluginTests
{
    [TestFixture]
    class YouTubeLiveServerTests
    {
        [Test]
        public void Test()
        {
            var server = new YouTubeLiveSitePlugin.Test2.YouTubeLiveServer();
            Assert.ThrowsAsync<HttpException>(async () => {
                await server.GetAsync("http://int-main.net/api/404");
            });
        }
    }
}
