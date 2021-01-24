using System;

namespace NicoSitePlugin
{
    public class ParseException : Exception
    {
        public string Raw { get; }
        public ParseException(string raw)
        {
            Raw = raw;
        }
        public ParseException(string message, string raw) : base(message)
        {
            Raw = raw;
        }
        public ParseException(string message, string raw, Exception inner) : base(message, inner)
        {
            Raw = raw;
        }
        protected ParseException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
