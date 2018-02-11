using System;
using NUnit.Framework;
using MultiCommentViewer.Test;
namespace MultiCommentViewerTests
{
    [TestFixture]
    public class LoggerTestTests
    {
        [Test]
        public void SerializeTest()
        {
            var ex = new Exception("テスト");
            var logger = new LoggerTest();
            logger.LogException(ex, "a", "b");
            var s = logger.GetExceptions();
            Assert.IsTrue(s.Contains("テスト"));
        }
    }
}
