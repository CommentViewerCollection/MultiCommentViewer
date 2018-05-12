using Moq;
using MultiCommentViewer;
using NUnit.Framework;
using Plugin;
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
