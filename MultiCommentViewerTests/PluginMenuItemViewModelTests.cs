using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiCommentViewer;
using NicoSitePlugin;
using Plugin;
using Moq;
using NUnit.Framework;
namespace MultiCommentViewerTests
{
    [TestFixture]
    class PluginMenuItemViewModelTests
    {
        [Test]
        public void プラグインメニューのコマンドが実行されると設定画面表示メソッドが呼ばれるか()
        {
            var mock = new Mock<IPlugin>();
            var vm = new PluginMenuItemViewModel(mock.Object);
            vm.ShowSettingViewCommand.Execute(null);
            mock.Verify(m => m.ShowSettingView());
        }
    }
}
