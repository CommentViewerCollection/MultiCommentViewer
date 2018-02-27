using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using MultiCommentViewer;
namespace MultiCommentViewerTests
{
    [TestFixture]
    class ConnectionNameTests
    {
        [Test]
        public void 接続名を変更すると通知が来る()
        {
            var b = false;
            var connectionName = new ConnectionName();
            connectionName.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(connectionName.Name):
                        b = true;
                        break;
                }
            };
            connectionName.Name = "test";
            Assert.AreEqual("test", connectionName.Name);
            Assert.IsTrue(b);
        }
    }
}
