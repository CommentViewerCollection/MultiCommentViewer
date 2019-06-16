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
            var optionsMock = new Mock<IOptions>();
            var options = optionsMock.Object;
            var vm = new PluginMenuItemViewModel(mock.Object, options);
            vm.ShowSettingViewCommand.Execute(null);
            mock.Verify(m => m.ShowSettingView());
        }
    }
}
