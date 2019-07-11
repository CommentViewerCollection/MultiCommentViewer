using Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTests
{
    [TestFixture]
    class DynamicOptionsBaseTests
    {
        class TestOptions : DynamicOptionsBase
        {
            public string Prop1 { get => GetValue(); set => SetValue(value); }
            public string Prop2 { get => GetValue(); set => SetValue(value); }

            protected override void Init()
            {
                Dict.Add(nameof(Prop1), new Item { DefaultValue = "1", Predicate = c => true, Serializer = c => c, Deserializer = s => s });
                Dict.Add(nameof(Prop2), new Item { DefaultValue = "2", Predicate = c => true, Serializer = c => c, Deserializer = s => s });
            }
        }
        [Test]
        public void DeserializeTest()
        {
            var s = "Prop1=8\r\nProp3=10";
            var options = new TestOptions();
            options.Deserialize(s);
            Assert.AreEqual("8", options.Prop1);
            Assert.AreEqual("2", options.Prop2);
        }
    }
}
