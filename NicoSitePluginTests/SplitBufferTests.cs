using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NicoSitePlugin.Test;
using NUnit.Framework;
namespace NicoSitePluginTests
{

    [TestFixture]
    class SplitBufferTests
    {
        ISplitBuffer2 _buf;
        [SetUp]
        public void Setup()
        {
            _buf = new SplitBuffer2("\0");
        }
        [Test]
        public void NicoLive_SplitBufferTest1()
        {
            var b1 = false;
            var s = "あいう\0えお\0";
            var bytes = Encoding.UTF8.GetBytes(s);
            _buf.Added += (sender, e) =>
            {
                Assert.AreEqual("あいう", e[0]);
                Assert.AreEqual("えお", e[1]);
                b1 = true;
            };
            _buf.Add(bytes, 0, 3);
            _buf.Add(bytes, 3, 1);
            _buf.Add(bytes, 4, bytes.Length - 4);
            Assert.IsTrue(b1);
        }
        [Test]
        public void NicoLive_SplitBufferTest2()
        {
            var b1 = false;
            var s = "あ\0う";
            var bytes = Encoding.UTF8.GetBytes(s);
            _buf.Added += (sender, e) =>
            {
                Assert.AreEqual("あ", e[0]);
                b1 = true;
            };
            _buf.Add(bytes, 0, 2);
            _buf.Add(bytes, 2, 2);
            _buf.Add(bytes, 4, bytes.Length - 4);
            Assert.IsTrue(b1);
        }
        [Test]
        public void NicoLive_SplitBuffer空文字あり()
        {
            var b1 = false;
            var b2 = false;
            var b3 = false;

            var s = "あい\0う\0\0";
            var bytes = Encoding.UTF8.GetBytes(s);
            var i = 0;
            _buf.Added += (sender, e) =>
            {
                if (i == 0)
                {
                    Assert.AreEqual("あい", e[0]);
                    b1 = true;
                }
                else if(i == 1)
                {
                    Assert.AreEqual("う", e[0]);
                    b2 = true;
                }
                else if(i == 2)
                {
                    Assert.AreEqual("", e[0]);
                    b3 = true;
                }
                i++;
            };
            Assert.AreEqual(12, bytes.Length);
            _buf.Add(bytes, 0, 2);
            _buf.Add(bytes, 2, 2);
            _buf.Add(bytes, 4, 3);
            _buf.Add(bytes, 7, 4);
            _buf.Add(bytes, 11, 1);
            Assert.IsTrue(b1 && b2 && b3);
        }
        [Test]
        public void SplitBufferSplitterと同じ要素のみ()
        {
            var b1 = false;
            var s = "\0";
            var bytes = Encoding.UTF8.GetBytes(s);
            _buf.Added += (sender, e) =>
            {
                Assert.AreEqual("", e[0]);
                b1 = true;
            };
            _buf.Add(bytes, 0, 1);
            Assert.IsTrue(b1);
        }
    }
}
