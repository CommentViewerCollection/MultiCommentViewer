using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using NUnit.Framework;
using SitePlugin;
using YouTubeLiveSitePlugin.Old;
using Moq;
namespace YouTubeLiveSitePluginTests.Old
{
    [TestFixture]
    public class CommentViewModelTests
    {
        [Test]
        public void 接続名を変更すると連動して通知する()
        {
            var optionsMock = new Mock<IOptions>();
            var connectionName = new ConnectionName() { Name = "a" };
            var cvm = new YouTubeCommentViewModel(connectionName, optionsMock.Object, new YouTubeSiteOptions());
            var b = false;
            cvm.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(cvm.ConnectionName):
                        b = true;
                        break;
                }                
            };
            connectionName.Name = "b";
            Assert.IsTrue(b);
        }
    }
}
