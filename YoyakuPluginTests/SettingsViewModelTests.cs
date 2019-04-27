using Moq;
using NUnit.Framework;
using OpenrecYoyakuPlugin;
using Plugin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoyakuPluginTests
{
    [TestFixture]
    class SettingsViewModelTests
    {
        [Test]
        public void ModelのTestResultの変更通知が来たら変更通知を出すかTest()
        {
            var optionsMock = new Mock<IOptions>();
            var hostMock = new Mock<IPluginHost>();
            var modelMock = new Mock<Model>(optionsMock.Object, hostMock.Object);
            var model = modelMock.Object;
            var vm = new SettingsViewModel(model, System.Windows.Threading.Dispatcher.CurrentDispatcher);
            var raised = false;
            vm.PropertyChanged += (s, e) =>
            {
                if(e.PropertyName == nameof(vm.TestResult))
                {
                    raised = true;
                }
            };
            modelMock.Raise(c => c.PropertyChanged += null, new PropertyChangedEventArgs(nameof(model.TestResult)));
            Assert.IsTrue(raised);
        }
    }
}
