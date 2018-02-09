using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using MultiCommentViewer.Test;
using System.Windows.Media;
namespace MultiCommentViewerTests
{
    [TestFixture]
    class DynamicOptionsTestTests
    {
        [Test]
        public void MCV_DynamicOptionsTest_SerializeDeserializeTest()
        {
            var optoins = new DynamicOptionsTest
            {
                BackColor = Colors.Aqua
            };
            var s = optoins.Serialize();
            var newOpt = new DynamicOptionsTest();
            newOpt.Deserialize(s);
            Assert.AreEqual(Colors.Aqua, newOpt.BackColor);
        }
    }
}
