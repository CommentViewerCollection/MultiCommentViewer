using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiCommentViewer;
using Moq;
using System.ComponentModel;
using SitePlugin;
using NUnit.Framework;
using Common;

namespace MultiCommentViewerTests
{
    [TestFixture]
    class ConnectionViewModelTests
    {
        [Test]
        public void ConnectionViewModel_RaiseRenamedWhenNameChanged()
        {
            var name = new ConnectionName();
            var conn = new ConnectionViewModel(name, new List<SiteViewModel>(), new List<BrowserViewModel>(), null);
            var newName = "new";
            var b = false;
            conn.Renamed += (s, e) =>
            {
                Assert.IsTrue(string.IsNullOrEmpty(e.OldValue));
                Assert.AreEqual(newName, e.NewValue);
                b = true;
            };
            conn.Name = newName;
            Assert.IsTrue(b);
        }
    }
}
