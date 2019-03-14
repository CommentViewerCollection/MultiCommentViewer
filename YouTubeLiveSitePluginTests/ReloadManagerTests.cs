using Moq;
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
    class ReloadManagerTests
    {
        Mock<ReloadManager> _managerMock;
        ReloadManager _manager;
        [SetUp]
        public void SetUp()
        {
            _managerMock = new Mock<ReloadManager>() { CallBase = true };
            _manager = _managerMock.Object;
        }
        public void SetTime(DateTime dateTime)
        {
            _managerMock.Setup(m => m.GetCurrentDateTime()).Returns(dateTime);
            _manager.SetTime();
        }
        public bool CanReload(DateTime dateTime)
        {
            _managerMock.Setup(m => m.GetCurrentDateTime()).Returns(dateTime);
            return _manager.CanReload();
        }
        public void SetCountLimit(int n)
        {

        }
        [Test]
        public void 一定時間内に規定回数以上のリロードは認められない()
        {
            _manager.CountLimit = 1;
            _manager.CountCheckTimeRangeMin = 1;
            Assert.IsTrue(CanReload(new DateTime(1970, 1, 1, 0, 0, 0)));
            SetTime(new DateTime(1970, 1, 1, 0, 0, 0));
            Assert.IsFalse(CanReload(new DateTime(1970, 1, 1, 0, 0, 0)));
        }
        [Test]
        public void 規定回数が0回の場合は一切リロードできない()
        {
            _manager.CountLimit = 0;
            _manager.CountCheckTimeRangeMin = 1;
            Assert.IsFalse(_manager.CanReload());
        }
        [Test]
        public void 規定回数が大きい数値でもOutOfRange等の例外を出さないか()
        {
            _manager.CountLimit = 100;
            _manager.CountCheckTimeRangeMin = 1;
            Assert.IsTrue(_manager.CanReload());
            SetTime(new DateTime(1970, 1, 1, 0, 0, 0));
            Assert.IsTrue(_manager.CanReload());
            SetTime(new DateTime(1970, 1, 1, 0, 2, 0));
            Assert.IsTrue(_manager.CanReload());
        }
        [Test]
        public void 一定時間を過ぎた場合はリロード回数がリセットされるか()
        {
            _manager.CountLimit = 1;
            _manager.CountCheckTimeRangeMin = 1;
            Assert.IsTrue(CanReload(new DateTime(1970, 1, 1, 0, 0, 0)));
            SetTime(new DateTime(1970, 1, 1, 0, 0, 0));
            Assert.IsTrue(CanReload(new DateTime(1970, 1, 1, 0, 2, 0)));
        }
    }
}
