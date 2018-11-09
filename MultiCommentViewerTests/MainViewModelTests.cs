using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SitePlugin;
using MultiCommentViewer;
using NUnit.Framework;
using Moq;
using System.ComponentModel;
using Common;
using MultiCommentViewer.Test;
using CommentViewerCommon;

namespace MultiCommentViewerTests
{
    [TestFixture]
    class MainViewModelTests
    {
        public void Test(Action<IOptions> action, string propertyName)
        {
            var ioMock = new Mock<IIo>();
            var loggerMock = new Mock<ILogger>();
            var optionsMock = new Mock<IOptions>();
            optionsMock.SetupSet(action).Raises(m => m.PropertyChanged += null, new PropertyChangedEventArgs(propertyName));
            var browserLoaderMock = new Mock<IBrowserLoader>();
            var siteLoaderMock = new Mock<ISitePluginLoader>();
            //var userStoreMock = new Mock<IUserStore>();
            var vm = new MainViewModel(ioMock.Object, loggerMock.Object, optionsMock.Object, siteLoaderMock.Object, browserLoaderMock.Object);
            vm.MainViewContentRenderedCommand.Execute(null);
            bool b = false;
            vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == propertyName)
                    b = true;
            };
            var prop = typeof(MainViewModel).GetProperty(propertyName);
            prop.SetValue(vm, true);
            Assert.IsTrue(b);
        }
        [Test]
        public void MainViewModel_IsShow_PropertyChanged()
        {
            Test(m => m.IsShowConnectionName = true, nameof(MainViewModel.IsShowConnectionName));
            Test(m => m.IsShowCommentId = true, nameof(MainViewModel.IsShowCommentId));
            Test(m => m.IsShowThumbnail = true, nameof(MainViewModel.IsShowThumbnail));
            Test(m => m.IsShowUsername = true, nameof(MainViewModel.IsShowUsername));
            Test(m => m.IsShowMessage = true, nameof(MainViewModel.IsShowMessage));
            Test(m => m.IsShowInfo = true, nameof(MainViewModel.IsShowInfo));
        }
    }
}
