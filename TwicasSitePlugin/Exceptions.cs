using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwicasSitePlugin
{
    [Serializable]
    internal class InvalidBroadcasterIdException : Exception
    {
        public string InvalidInput { get; }
        public InvalidBroadcasterIdException(string input)
        {
            InvalidInput = input;
        }
    }
    [Serializable]
    internal class SpecChangedException : Exception
    {
        public string Raw { get; }
        public SpecChangedException() { }
        public SpecChangedException(string message, string raw) : base(message)
        {
            Raw = raw;
        }
        public SpecChangedException(string message, Exception inner) : base(message, inner) { }
        protected SpecChangedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
