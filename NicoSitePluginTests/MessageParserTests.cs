using NicoSitePlugin.Websocket;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NicoSitePluginTests
{
    [TestFixture]
    class MessageParserTests
    {
        MessageParser2 _parser;
        [SetUp]
        public void SetUp()
        {
            _parser = new MessageParser2();
        }
    }
}
