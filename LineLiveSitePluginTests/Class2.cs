using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LineLiveSitePlugin;
using SitePlugin;
using Moq;
using Common;
using ryu_s.BrowserCookie;
using System.Net;

namespace LineLiveSitePluginTests
{
    [TestFixture]
    class Class2
    {
        class Server : IDataServer
        {
            public async Task<string> GetAsync(string url)
            {
                if (url == "https://live-api.line-apps.com/web/v2.5/billing/gift/loves")
                {
                    return GetSampleData("Loves.txt");
                }
                else if (url == "https://live-api.line-apps.com/app/v2/channel/2577702/broadcast/8680302")
                {
                    return GetSampleData("LiveInfo.txt");
                }
                throw new NotImplementedException();
            }

            public Task<string> GetAsync(string url, Dictionary<string, string> headers, CookieContainer cc)
            {
                throw new NotImplementedException();
            }

            private string GetSampleData(string filename)
            {
                var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"SampleData\" + filename);
                string sample = "";

                using (var sr = new System.IO.StreamReader(path))
                {
                    sample = sr.ReadToEnd();
                }
                return sample;
            }
        }
        [Test]
        public async Task Test()
        {
            var optionsMock = new Mock<ICommentOptions>();
            var loggerMock = new Mock<ILogger>();
            var browserProfileMock = new Mock<IBrowserProfile>();
            ISiteContext context = new LineLiveSiteContext(optionsMock.Object, new Server(), loggerMock.Object);
            var commentProvider = context.CreateCommentProvider();

            commentProvider.CommentReceived += (s, e) =>
            {

            };
            await commentProvider.ConnectAsync("https://live-api.line-apps.com/app/v2/channel/2577702/broadcast/8680302", browserProfileMock.Object);



        }

    }
}
