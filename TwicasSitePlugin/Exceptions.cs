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
        public InvalidBroadcasterIdException() { }
        public InvalidBroadcasterIdException(string message) : base(message) { }
        public InvalidBroadcasterIdException(string message, Exception inner) : base(message, inner) { }
        protected InvalidBroadcasterIdException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
    [Serializable]
    internal class SpecChangedException : Exception
    {
        public SpecChangedException() { }
        public SpecChangedException(string message) : base(message) { }
        public SpecChangedException(string message, Exception inner) : base(message, inner) { }
        protected SpecChangedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
